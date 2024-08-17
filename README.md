# PuzBobClone

A clone implementation based on "Puzzle Bobble" game, in Unity.
Mainly built for personal understanding of mechanics involved in dealing with balls and hexagonal grid.

## Features

- proper 'orange stacking' in (hexagonal) grid
  - worked out positions with some basic trig, see [DESIGN_NOTES.md](DESIGN_NOTES.md)
  - when balls are played, snap them to exact positions in grid
- add shooting guide, and if needed, guide for first reflection
- detect and pop chain of 3-or-more balls of same color
- when all of a ball color is cleared, no longer randomly generate it at shooter
- start with one of several level layouts, when complete/fail/reset stage
- added basic text fields, showing controls, ball count, etc.

### Code points

- `GridPositions` library to convert between world coords (`Vector3`), and grid positions (`Vector2Int`)
  - also stores played ball locations and colors, for pop checking, etc.
- right now most logic is driven via `Ball` collision, and `BallShooter` input handling
  - model wouldn't work so well, if need to add animations, etc.
  - further enhancements could benefit from game state driven centrally, e.g. thru `GameManager`
