# SignPAK alphabet animation drafts

The Unity assets are in `Assets/Animations/English Alphabets/`. A and B were retained; C through Z were added. Every clip is a three-second humanoid clip, derived from the timing and body pose in A. The unused arm uses the resting pose established in B. J and Z have arm/wrist motion for their traced paths.

Reference family: the one-handed first variants in the [FESF PSL English alphabet dictionary](https://psl.org.pk/dictionary/101-english-alphabet). The clips are manually authored approximations, not motion captures. Several reference videos started at rest, so some contact details were inferred from the dictionary handshape and require confirmation against the full performance.

Validation: all 26 assets loaded in Unity, had length 3 seconds, were humanoid, and contained no NaN or infinite curve values. Each was sampled and rendered on Jack. This verifies the assets and visual poses, not linguistic correctness or playback on every avatar.

Review K, M, N, T and R closely: thumb placement, finger overlap, and contact depend on the avatar. Check the direction and timing of J/Z against your chosen PSL performance. The clips have not been added to the sign database or Animator states.

`Alphabet-review.png` shows hand close-ups at 1.5 seconds. `J-motion.png` and `Z-motion.png` show four movement frames. Full-body and individual hand previews are also saved here.
