# Tetris, but Triminos #

Uses a RL function approximation algorithm to clear as many tritrises! 
- a tritris is when 3 lines are cleared at once

# Game Spec

This is done on a reduced 4x6 board, with random seeds of pieces -- no rotation!

The commands are
- left
- right
- hard drop

No tucks or gravity!

# Implementation of RL

The following states are encoded by a 24 length bitvector, followed by a 6 length bitvector of the piece.
Finally, we need to encode the current position of the piece, which is stored as the center with 2 integers.

To summarize, we have the following feature vector
[ bool[24] board -- bool[6] piece_type -- int[2] position ]

We shall use a linear function parameterized by w, where we minimize the squared distance of v(s, w) = w dot x(s) and bootstrapped g_t (n-step Q-learning).
This is the standard function approximation (SGD) algorithm discussed in https://www.youtube.com/watch?v=Vky0WVh_FSk.

Further, we shall positively reward on every tritris and have a very strong negative reward for topping out.

Also, we shall choose n to be relatively small, around 3 or 4.
We do not want n to be too small, as then the negative reward of topping out will take longer to trickle down.

