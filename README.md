# Framewerk

Inversion of control MVCS micro-framework with dependency injection.

Framewerk is heavily influenced by Robotlegs and while its not trying to be its Unity port, its purpose is to bring similar workflow to Unity. 
Main benefit of this design principle is creating modular, highly extensible and reusable code.
Although core part of Framewerk could be used in any C# project, significant part of it is also view mediation mechanism which is Unity specific
and on top of that there are UI supporting classes covering basic concepts like Popups, Lists, Tabs and other ui elements.

## Quick Start

 - Download project and open it in Unity 2017 or newer.
 - Open SampleScene and hit Play. You should get to the main screen with several examples 
 - All examples are done in the simplest form possible so I recommend going through them to get the workflow.
 
## Core
There are three core parts of the framework: **Injector**, **EventDispatcher** and **CommandMap**. 
Although there are more crucial parts which will help you to kick-start a new project and do something working fast without fear that you'll have to rewrite lot of stuff later, but these three are basic building block you'll probably in every project using Framewerk.
  
### Injector
This class will help you to setup your dependencies through whole project easily and efficiently.
Just feed it all your models, managers and tools you need to access from all different parts of your codebase and then just mark fields you want to be automatically filled by **Injector** with **[Inject]** Attribute.
  
### EventDispatcher
Simple Implementation of many-to-many Observer pattern, allowing you to communicate between all different parts of your application easily via Events.

### ComandMap
**CommandMap** simply maps **Commands** to **Events**. **Commands** are small pieces of **Controller's** layer focused performing one task, which allows for modularity and easy reusability.
With  **CommandMap** you can trigger any **Commands** from any part of your application simply by dispatching corresponding **Event**.

### Context
Oh and then there is is this guy. Its your bootstrap file, where you can setup all injections and mapping.
**Context** is simply setup class telling Framewerk how all bits and pieces making up your game or app should be wired together.

## Future
Framewerk in a current state is my go-to toolset i use in every Unity project. It was created and slowly evolved to current form being used in several projects.
Compared to polished frameworks like [Robotlegs](https://github.com/robotlegs/robotlegs-framework) and [StarngeIoC](http://strangeioc.github.io/strangeioc/) or even robust dependency injection only framework like [Zenject](https://github.com/svermeulen/Zenject), 
Framewerk is simpler with less features and the plan is to keep it that way, but there are definitely features that will be added in future. To name few comming soon:

- Constructor injection
- Non-singleton injection(as now Injector can't construct and provide new instance with every injection)
- Named injections
- Better support of multiple contexts

After these will be done, proper documentation, and couple of tutorials covering most of core functionality will be done before adding more stuff to the framework.

[![GitHub Super-Linter](https://github.com/dyskotron/Framewerk/workflows/Lint%20Code%20Base/badge.svg)](https://github.com/marketplace/actions/super-linter)
