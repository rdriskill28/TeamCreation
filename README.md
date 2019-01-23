# TeamCreation
Create evenly rated teams from a list of player names &amp; ratings

Accepts a .csv file with 2 columns (Name, Rating).  Choose how many players you want per team and it will create as evenly rated teams as possible.  

Logic:

1. Get the average rating of all players
2. Remove the highest rated player from the list of players (reduces permutations for next step)
3. Get all possible permutations for the player list with 1 fewer players that needed.
4. Find the team that is closest to the player average once we add the highest player back to that team
5. Create this team and remove players in this team from the list.  Repeat steps 2-5 until all players are used up.



Note:  This project was created very quickly to help a friend run an event.  It was also my first attempt at working with WPF.  I'm sure it could be vastly improved, but I am fairly happy with the speed of the team creation logic.  It was much slower in the first version I made. 
