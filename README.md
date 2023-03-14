# Turtle Puzzle Solver
A project made specifically to solve an old puzzle - a 4x4 card puzzle with colored turtles.

Photo of original puzzle with one solution can be found in [Stuff](https://github.com/therealtkh/TurtlePuzzleSolver/tree/main/Stuff).

The solution in the image was used as reference when creating the cards (order 0-15), they have one "base" slot each for when not placed on the field. This can be seen when starting the program:

![Welcome Screen](https://github.com/therealtkh/TurtlePuzzleSolver/blob/main/Stuff/example_1.png)

When pressing the Start button, the program will cycle through all possible combinations and rotating cards systematically. When a chain reaches its end it will be accounted for and then falling back to last "fork" and try another combination. Sometimes a chain fails after a few cards, but quite a few reached all the way!

Note that since every card will be rotated and tested in four directions, four times as many combinations and solutions will be found. Once the algorithm has finished, another function will run and do two things: 
1. Rotate all solutions until card 5 (five) has correct rotation (any card would work)
2. Delete all duplicate solutions (easier to do if field rotation is based on a specific card)

![Finished Run](https://github.com/therealtkh/TurtlePuzzleSolver/blob/main/Stuff/example_2.png)

Once finished with removal of duplicates, all solutions are presented and can be cycled through with the Solution button. The text on the button is updated for each solution.

At any time during the run, the user can press the Stop button. This was made possible by using a Background Worker, separating the recursive function from the UI thread. If pressing Stop, cycling through solutions will still work.

Hopefully, code and algorithm is correct. It's probably not the fastest way to do it, but works fine (I think)! The solution is also very slow because I decided to draw a combination every so often. Reporting in the log is of course also optional but I think it's fun to see the progress.

Thanks to **Carl** and **Anders** for various types of support! Carl's old version of "not rotating cards" is left in the code, as well as my own first version that didn't rotate cards (with added _Old_ in the function name).
