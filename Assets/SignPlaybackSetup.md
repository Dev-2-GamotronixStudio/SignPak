# Test sign animations

Open `Assets/Scenes/Main.unity` and enter Play mode. Elizabeth is the active test character.

- Click any A–Z button to play that letter.
- Space plays all 26 letters; Left/Right steps through them; Escape stops playback. Give the Game view keyboard focus first. Shortcuts are disabled while editing the text field.
- Enter a sequence such as `ABC` or `BB`, then click **Play Letters**. Spaces are ignored.

# Use another character

1. Import the character using the **Humanoid** animation type and configure a valid Avatar. Map all three joints of each finger for signing.
2. Assign `Assets/Animations/AnimationController.controller` to its Animator. The controller contains `Idle` and `PerformSign`; the latter uses `SignPlaceholder.anim`, which the player replaces at runtime.
3. Add `SignPlaybackController` to the character or a manager. Assign its **Target Animator** and `Assets/ScriptableObjects/SignDatabase.asset`. If Target Animator is empty, the player searches its own object and children.
4. Point `AlphabetSequenceTester`'s **Playback Controller** at that player's component. Activate the chosen character. Jack is already configured but remains inactive in Main.

Each player creates its own Animator Override Controller, so separate characters can play different signs without changing the shared controller. Root motion is disabled during sign playback. Database playback speeds are respected.

Humanoid retargeting shares clips across valid Avatars; Generic rigs need separate animation authoring. Hand proportions and Avatar muscle limits can change fingertip contact, so review each new character's signing poses.

# Fix and verification

The original PerformSign state had no motion, so there was no SignPlaceholder key for the override. The keyboard Update method was also commented out. The placeholder and idle motions are now assigned; the tester supports the project's new Input System and exposes buttons for every letter.

Play-mode checks selected all 26 clips on both Elizabeth and Jack (52 character/clip combinations), verified finite transforms and the expected Animator state/clip, checked that A/B finger rotations differed, and verified repeated-letter restart and return to Idle. The recorded results are in `AlphabetPreviews/Playback-validation.csv`. These checks establish playback, not linguistic correctness of the draft sign poses.
