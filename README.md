# EDUS
Elite Dangerous Undiscovered Systems

A tool to help Elite Dangerous players find undiscovered star sytems near them.

## Goal

Use machine learning, and the existing elite dangerous tools, to calculate where the nearest undiscovered star is to a players position (or star of their choosing). This is in order to allow players who enjoy exploring to more easily find new things, and to allow the average player to more easily find an undiscovered or scanned target near them that they can scan in order to get their name on a planet or star.

### Stretch Goals

1. Ideally I'd like it if I could even get it to predict where the most valuable objects would be based on the likely system name, as all system names signify their contents.
2. I'd also like to add a pathing tool to go from one star to another, with the most quantity/most expensive undiscovered systems put on that path
3. I'd like for it to also be able to find unscanned objects in already discovered systems, as some players may just want to be "First scanned" rather than "First discovered" and don't want to have to leave the bubble.

## Tech used

ASP.Net
SQL Server Management Studio
NPG Sql

I do use the following Elite Dangerous community tools to get the training data, and encourage players to add their logs:
1. EDSM
2. EDDB
3. Inara

### Algorithm Technique

#### Training

1. Process each discovered star in order of discovery, and make a guess about the next star in the list.
2. Pull the next star and check how accurate the guess was.
3. Adjust weights/biases and go to the next star.
4. For every new star that is discovered and added to EDSM, use it as a training data point even while live, to become more accurate.

This does mean that it's less discovering the nearest new star, and more discovering the nearest star that it thinks players would have next discovered on their own within a section of space. This is why creating a list of stars ordered to the player's search criteria, and displaying confidence in them being where predicted, is the way it will be presented to the end user. It also has the consequence that the more players use the tool and trust it, the less they're training the algorithm, and the more the algorithm is training players to reinforce it's own training, which is weird.

#### Final Product

1. Create a cube in space centered around the chosen search sytem using the system's coordinates and search criteria. 
2. Pull from the database all systems in that cube that have been discovered.
3. Predict the coordinates of n amount of systems within that cube, in decreasing order of confidence, using the pulled data and any search criteria.
4. Predict the systems names, in order to present them to the player to search for the system in game.
5. Display results.

## Bugs/Challenges

This project is currently on hold, as while it does currently work, it takes far too long to train using the CPU due to the magnitude of all the systems discovered and the magnitude of all undiscovered systems. I will try to finish this project after learning GPU processing for the linear algebra needed, or after finding a good C# library for GPU machine learning.
