# English recording provenance

The twenty WAV clips were generated locally with Kyutai Pocket TTS and split
from complete recordings of the digits zero through nine. The project owner
supplied and reviewed the resulting clips.

Preset mapping:

| Resource folder | Pocket TTS preset |
| --- | --- |
| Male | alba |
| Female | anna |

Processing: split at pauses, retained short silence around each digit, and
exported as mono, 24 kHz, 16-bit PCM WAV. The original combined recordings and
model weights are not included in this package. No endorsement by the voice
speakers or Kyutai is implied.

Upstream attribution:

- Kyutai, Pocket TTS: https://github.com/kyutai-labs/pocket-tts
- Model card (CC BY 4.0): https://huggingface.co/kyutai/pocket-tts
- Model license: https://creativecommons.org/licenses/by/4.0/
- Pocket TTS software license (MIT): https://github.com/kyutai-labs/pocket-tts/blob/main/LICENSE

The provider source code is covered by the repository's Apache-2.0 license.
This notice preserves attribution to the tool and model used to generate the
recordings; it does not claim that the model weights are distributed here.
