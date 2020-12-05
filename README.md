# First Person Engine
This project is an open-source, first-person shooter framework, written in C# for Unity.
The repository is structured as a Unity project, and there are example levels included for demonstrating the gameplay components.

# Installation and Setup
Setup should be minimal. Clone this repo and place it in a folder of your choosing. You will need Unity 2020.1.5f1 or newer.
Then, add the project in Unity Hub, and import the project.

# Code Overview
The code is structured in the Assets/Scripts folder. There are two major groups of code structure; "Components" and "Behaviors". These are contained in their respective folders.

Components represent a systemic, predictable API for interacting with the framework. These are meant to be added to Unity gameobjects, and to have functions called on them to perform behavior. For example, a Gun Component might have a Shoot() function, and when called, will shoot a bullet into the world. These components also have data on them.

Behaviors are less restrictive, and represent the open ended nature of video game scripting. They are meant to be a bridge between the more rigid, systemic components, and the variety of actions that occur in games. For example, an Enemy Behavior might be responsible for playing animations, making sounds, and firing a gun (All of which are components, and therefore systemic). Behaviors often make a variety of calls to components to accomplish their tasks.

# Feature List
Billboard Sprites Components, used for 2D sprites in 3d space, much like Doom or Wolfenstein 3D.
* Material Animation
* Mesh Bounds
* Rotatable

Enemy Components, used for managing enemies and their actions
* Attack Token
* Bark
* Enemy Manager

Level Components, used for level loading and lighting changes
* Level Lighting Volume
* Level Manager
* Level Trigger

Player Components TODO
* ...
* ...

Shooting Components, used for shooting a bullet and dealing damage to damageables
* Bullet
* Damageable
* FlatAnimatedGun
* Gun
* Gun Selection
* Zoomable Gun

Sound Components, used for managing the sounds within the game
* Ambient Sound
* Sound
* Sound Manager

# Future Work
TODO
