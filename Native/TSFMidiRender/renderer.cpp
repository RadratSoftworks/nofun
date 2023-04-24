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
 
#define TSF_IMPLEMENTATION
#define TML_IMPLEMENTATION

#include "tsf/tsf.h"
#include "tsf/tml.h"
#include <cstdint>
#include <vector>

#ifdef _MSC_VER
#define TMR_EXPORT __declspec(dllexport)
#else
#define TMR_EXPORT
#endif

static tsf *g_base_tsf = nullptr;
static int g_freq = 0;

struct PlayResource {
    tml_message *messages = nullptr;
    tml_message *orgMessages = nullptr;
    PlayResource *next = nullptr;
    tsf *synth = nullptr;
    double msecs = 0;
    bool loop = false;

    explicit PlayResource(tml_message *messages, bool loop = false)
        : messages(messages)
        , orgMessages(messages)
        , loop(loop) {
        synth = tsf_copy(g_base_tsf);
    }

    explicit PlayResource() = default;
};

static PlayResource g_resourceList;
static PlayResource *g_lastResource = nullptr;
static bool g_donePlayingSaw = false;
static std::vector<PlayResource *> g_donePlaying;

extern "C" TMR_EXPORT int nofunTSFStartup(const char *soundFontData, const uint32_t soundFontSize, const int outputSampleRate) {
    g_base_tsf = tsf_load_memory(soundFontData, soundFontSize);
    if (g_base_tsf == nullptr) {
        return -1;
    }

    tsf_channel_set_bank_preset(g_base_tsf, 9, 128, 0);
	tsf_set_output(g_base_tsf, TSF_STEREO_INTERLEAVED, outputSampleRate, 0.0f);

    g_freq = outputSampleRate;
    return 0;
}

extern "C" TMR_EXPORT void nofunTSFShutdown() {
    PlayResource *resource = g_resourceList.next;
    while (resource) {
        tml_free(resource->orgMessages);
        tsf_close(resource->synth);

        PlayResource *current = resource;
        resource = resource->next;

        delete current;
    }

    tsf_close(g_base_tsf);
}

extern "C" TMR_EXPORT void *nofunTSFLoad(const char *midiData, const uint32_t midiDataSize, const int loop) {
    tml_message *msg = tml_load_memory(midiData, midiDataSize);
    if (msg == nullptr) {
        return nullptr;
    }
    PlayResource *resource = new PlayResource(msg, (bool)loop);
    if (g_lastResource != nullptr) {
        g_lastResource->next = resource;
    } else {
        g_resourceList.next = resource;
    }
    g_lastResource = resource;
    return resource;
}

void nofunTSFFreeImpl(void *handle, bool deleteFromDonePlaying = false) {
    PlayResource *resource = (PlayResource *) handle;
    if (resource == nullptr) {
        return;
    }

    PlayResource *resourceIter = &g_resourceList;
    while ((resourceIter != nullptr) && (resourceIter->next != resource)) {
        resourceIter = resourceIter->next;
    }

    if (resourceIter != nullptr) {
        resourceIter->next = resource->next;

        if (resourceIter->next == nullptr) {
            g_lastResource = (resourceIter == &g_resourceList) ? nullptr : resourceIter;
        }
    }

    if (deleteFromDonePlaying) {
        auto ite = std::find(g_donePlaying.begin(), g_donePlaying.end(), resource);
        if (ite != g_donePlaying.end()) {
            g_donePlaying.erase(ite);
        }
    }

    tml_free(resource->orgMessages);
    tsf_close(resource->synth);

    delete resource;
}

extern "C" TMR_EXPORT void nofunTSFFree(void *handle) {
    nofunTSFFreeImpl(handle, true);
}

extern "C" TMR_EXPORT void *nofunTSFGetDonePlayingHandles(int *count) {
    *count = (int)g_donePlaying.size();
    if (g_donePlaying.size() == 0) {
        return nullptr;
    }

    void *returnResult = g_donePlaying.data();

    for (PlayResource *resource : g_donePlaying) {
        nofunTSFFreeImpl(resource, false);
    }

    g_donePlayingSaw = true;
    return returnResult;
}

extern "C" TMR_EXPORT void nofunTSFGetBuffer(float *sampleData, int sampleCountOrg) {
    PlayResource *resource = g_resourceList.next;

    if (g_donePlayingSaw) {
        g_donePlaying.clear();
        g_donePlayingSaw = false;
    }

    while (resource != nullptr) {
        if (resource->messages == nullptr) {
            resource = resource->next;
            continue;
        }

        int sampleBlock, sampleCount = sampleCountOrg; //2 output channels
        float *stream = sampleData;

        for (sampleBlock = TSF_RENDER_EFFECTSAMPLEBLOCK; sampleCount; sampleCount -= sampleBlock, stream += sampleBlock * 2) {
            //We progress the MIDI playback and then process TSF_RENDER_EFFECTSAMPLEBLOCK samples at once
            if (sampleBlock > sampleCount) sampleBlock = sampleCount;

            //Loop through all MIDI messages which need to be played up until the current playback time
            for (resource->msecs += sampleBlock * (1000.0 / g_freq); resource->messages && resource->msecs >= resource->messages->time; resource->messages = resource->messages->next) {
                switch (resource->messages->type) {
                    case TML_PROGRAM_CHANGE: //channel program (preset) change (special handling for 10th MIDI channel with drums)
                        tsf_channel_set_presetnumber(resource->synth, resource->messages->channel, resource->messages->program, (resource->messages->channel == 9));
                        tsf_channel_midi_control(resource->synth, resource->messages->channel, TML_ALL_SOUND_OFF, 0);
                        break;
                    case TML_NOTE_ON: //play a note
                        tsf_channel_note_on(resource->synth, resource->messages->channel, resource->messages->key, resource->messages->velocity / 127.0f);
                        break;
                    case TML_NOTE_OFF: //stop a note
                        tsf_channel_note_off(resource->synth, resource->messages->channel, resource->messages->key);
                        break;
                    case TML_PITCH_BEND: //pitch wheel modification
                        tsf_channel_set_pitchwheel(resource->synth, resource->messages->channel, resource->messages->pitch_bend);
                        break;
                    case TML_CONTROL_CHANGE: //MIDI controller messages
                        tsf_channel_midi_control(resource->synth, resource->messages->channel, resource->messages->control, resource->messages->control_value);
                        break;
                }
            }

            // Render the block of audio samples in float format
            tsf_render_float(resource->synth, (float*)stream, sampleBlock, 1);

            if (resource->messages == nullptr) {
                if (resource->loop) {
                    resource->messages = resource->orgMessages;
                    resource->msecs = 0;
                } else {
                    g_donePlaying.push_back(resource);
                    break;
                }
            }
        }

        resource = resource->next;
    }
}