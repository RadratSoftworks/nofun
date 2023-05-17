/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#include <cstdio>
#include <vector>

extern "C" {
  // Android file in kernel? go through some layer, that writing to any position will
  // truncate the file at the seek position. Very weird
  // So far using rb+ should be correct, but this behaviour is absurb
  struct SAFFile {
    FILE *handle;

    std::vector<char> write_buffer;
    long write_start_position;
    bool write_flushed;
    
    explicit SAFFile(FILE *handle)
      : handle(handle)
      , write_start_position(-1)
      , write_flushed(true)
    {
      fseek(handle, 0, SEEK_END);
      write_start_position = ftell(handle);
      fseek(handle, 0, SEEK_SET);
    }
    
    void flush_write() {
      if (!write_flushed) {
        long current = ftell(handle);
        fseek(handle, write_start_position, SEEK_SET);
        fwrite(write_buffer.data(), write_buffer.size(), 1, handle);
        fseek(handle, current, SEEK_SET);
        
        write_flushed = true;
      }
    }
    
    ~SAFFile() {
      if (!write_flushed) {
        flush_write();
      }

      if (handle != nullptr) {
        fclose(handle);
      }
    }
  };
  
  void *saf_router_open(int fd) {
    FILE *f = fdopen(fd, "rb+");
    if (!f) {
      return nullptr;
    }
    
    return new SAFFile(f);
  }
  
  int64_t saf_router_read(void *file, void *buffer, int count) {
    return (int64_t)fread(buffer, 1, count, ((SAFFile*)file)->handle);
  }
  
  int64_t saf_router_write(void *file, const void *buffer, int count) {
    SAFFile *safFile = (SAFFile*)file;
    long position = ftell(safFile->handle);
    
    if (position < safFile->write_start_position) {
      std::vector<char> append_buf(safFile->write_start_position - position);
      fread(append_buf.data(), append_buf.size(), 1, safFile->handle);
      
      safFile->write_start_position = position;
      safFile->write_buffer.insert(safFile->write_buffer.begin(), append_buf.begin(), append_buf.end());
      
      fseek(safFile->handle, position, SEEK_SET);
    }
    
    std::memcpy(safFile->write_buffer.data() + position - safFile->write_start_position, buffer, count);
    
    fseek(safFile->handle, position + count, SEEK_SET);
    safFile->write_flushed = false;

    return count;
  }
  
  int64_t saf_router_tell(void *file) {
    return ftell(((SAFFile*)file)->handle);
  }
  
  void saf_router_flush(void *file) {
    SAFFile *safFile = (SAFFile*)file;
    safFile->flush_write();

    fflush(((SAFFile*)file)->handle);
  }
  
  int64_t saf_router_seek(void* file, int offset, int where) {
    FILE *handle = ((SAFFile*)file)->handle;
    fseek(handle, offset, where);
    return ftell(handle);
  }
  
  void saf_router_close(void *file) {
    fclose(((SAFFile*)file)->handle);
  }
}