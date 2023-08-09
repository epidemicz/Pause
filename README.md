# Pause
A simple SPT mod which allows you to pause the game including the raid timer. 

Mostly cobbled together from Kobrakon's [Take-a-Break](https://github.com/kobrakon/TakeABreak) as a learning exercise.

Works with SPT 3.6.0

## What gets paused
- You
  - Character control
  - Health
  - Hydration & Energy
  - Stamina
- AI
- The world/time of day (I think)
- The actual game raid timer
- The fake raid timer you see when you press o

## Stuff that doesn't really pause well now
Stuff that doesn't pause well at the moment and may not be worth the effort.
- Ragdolls, physics stuff in general
  - Grenades will fly but the fuses won't tick until you unpause
- You jumping (instant ice skating)
- Certain oxygen/stamina effects
  - Pause while aiming will continue to drain oxygen
  - Pause while stamina is recovering will continue to regen
- You can pause and still move inventory around (shouldn't be able to use anything)
  
## Todo & Notes
- Do a code style pass and random cleanup
- Pause audio?
  - AudioListener gameObject on DontDestroyOnLoad, set its pause field to true
- patch GClass714.Update to stop oxygen while ads (prob stam regen too)