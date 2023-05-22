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

extern "C" {
  struct SAFFile {
    FILE *handle;
    
    explicit SAFFile(FILE *handle)
      : handle(handle)
    {
    }
    
    ~SAFFile() {
      if (handle != nullptr) {
        fclose(handle);
      }
    }
  };
  
  void *saf_router_open(int fd) {
    FILE *f = fdopen(fd, "rb");
    if (!f) {
      return nullptr;
    }
    
    return new SAFFile(f);
  }
  
  int64_t saf_router_read(void *file, void *buffer, int count) {
    SAFFile *safFile = (SAFFile*)file;
    return (int64_t)fread(buffer, 1, count, safFile->handle);
  }
  
  int64_t saf_router_tell(void *file) {
    return ftell(((SAFFile*)file)->handle);
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