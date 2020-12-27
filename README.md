# First Person Engine
This project is an open-source, first-person shooter framework, written in C# for Unity.
The repository is structured as a Unity project, and there are example levels included for demonstrating the gameplay components. This is a framework applicable for many different types of shooter games, but it's especially suited for retro or "boomer" shooters, including code for 2d guns, sprite enemies, and various map import options.

![example scene](https://i.imgur.com/mDLxd9D.png)

# Installation and Setup
Setup should be minimal. Clone this repo and place it in a folder of your choosing. You will need Unity 2020.1.5f1 or newer.
Then, add the project in Unity Hub, and import the project.

# Current Feature List
Shooting Components, used for shooting a bullet and dealing damage to damageables
* 2D and 3D animated gun components
* Gun, Bullet, and Damageable Components
* Simple Gun Selection

Player Components, for managing the first person player
* First Person Player, with movement, aiming, player sounds, and death/respawning

Billboard Sprites Components, used for 2D sprites in 3d space, much like Doom or Wolfenstein 3D.
* Rotatable, with Animation

Enemy Components, used for managing enemies and their actions
* Enemy Manager
* Enemy Behavior
* Sprite Enemy Behavior
* Attack Token
* Bark

Level Components, used for generating levels from common level editor programs, level loading, and lighting changes
* Tiled Builder (Import *.csv files from Tiled)
* Level Manager
* Level Trigger
* Level Lighting Volume

Sound Components, used for managing the sounds within the game
* Sound Manager, with Sound Component and Ambient Sound Manager

Libraries, for helping with general tasks
* Custom Math
* Localizer
* Timer
* Pooled GameObject Manager
* Console output to game with tilde key

# Code Overview
The code is structured in the \Assets\Scripts folder. There are two major groups of code structure; "Components" and "Behaviors". These are contained in their respective folders.

Components represent a systemic, predictable API for interacting with the framework. These are meant to be added to Unity GameObjects, and to have functions called on them to perform behavior. A gameobject might have many different components on it. For example, a Gun Component might have a Shoot() function, and when called, will shoot a bullet into the world. These components also have data on them.

Behaviors are less restrictive, and represent the open ended nature of video game scripting. They are meant to be a bridge between the more rigid, systemic components, and the variety of actions that occur in games. For example, an Enemy Behavior might be responsible for playing animations, making sounds, and firing a gun (All of which are components, and therefore systemic). Behaviors often make a variety of calls to components to accomplish their tasks. Usually, only one of these is added to a given GameObject, and the naming of these behaviors is relevant to the specific behavior it implements. For example, a Sniper Enemy might have a SniperEnemyBehavior, and it's only used for them.

A good rule of thumb: Components can talk to other components, but components should have no knowledge of behaviors.

# Future Work
* Clean up the TODOs
* Implement 3D variants for all the sprite-based components (example 3d enemy)
* Settings Manager system
* STRETCH GOAL: WAD file importing
