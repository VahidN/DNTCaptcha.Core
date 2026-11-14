# Persian recording provenance

The project owner supplied `persian-0-9.wav`, generated locally with the default
female voice of `ArminRahimi/Audio8-TTS-Persian`. It contains the words
صفر، یک، دو، سه، چهار، پنج، شش، هفت، هشت، نه in that order.

The ten clips retain the supplied mono, 24 kHz, 16-bit PCM samples. Processing
removed the nine 700 ms digital-silence separators, trimmed low-level tails
using 10 ms RMS windows at a threshold of 50 PCM units, and retained a 50 ms
buffer on each side of detected activity. No normalization, resampling, or
new synthesis was applied. Short buffers outside the source are zero padded.

The combined source recording and model weights are not included in this package.

Upstream attribution:

- Persian model and model card (Apache-2.0):
  https://huggingface.co/ArminRahimi/Audio8-TTS-Persian
- Persian inference and training code (Apache-2.0):
  https://github.com/ard9/Audio8-Persian-TTS
- Base model, Audio8-TTS-Preview-0.6B:
  https://huggingface.co/Edge0/Audio8-TTS-Preview-0.6b
- Training datasets listed by the model author (CC0-1.0):
  https://huggingface.co/datasets/MahtaFetrat/Mana-TTS
  https://huggingface.co/datasets/MahtaFetrat/GPTInformal-Persian

The provider source code is covered by the repository's Apache-2.0 license.
No endorsement by the model authors or speakers is implied.
