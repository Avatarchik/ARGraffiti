# ARGraffiti App Readme
### Create and Save graffiti in the real world with Placenote SDK

AR Graffiti is our version of Tilt Brush for Augmented Reality. The problem with AR painting apps that use GPS is that you cannot save paintings in precise positions in the real world. For e.g. if you were drawing a mustache on a statue, there's no way to save that mustache unless you can detect the statue and precisely position content relative to it. 

Placenote SDK provides an easy way to add mapping and precise persistent content placement to your ARKit app. This project shows you how to build a persistent painting app with ARKit and PlacenoteSDK.

## Getting Started
To install ARGraffiti, follow these instructions:

* Clone this repository
* Critical library files are stored using lfs, which is the large file storage mechanism for git.
  * To Install these files, install lfs either using HomeBrew:
  
     ```Shell Session 
     brew install git-lfs
     ```

      or MacPorts: 
      ```Shell Session
      port install git-lfs
      ```
   
  * And then, to get the library files, run: 
     ```Shell Session
     git lfs install 
     git lfs pull
     ```
* Open the project as a new project in Unity (Recommended: Unity 2017)
* Make sure you have an API key. [Get your API key here](https://developer.placenote.com)
* To build and run the project on your iOS device, follow these [Build Instructions](https://placenote.com/install/unity/build-unity-project/)


## How does the app work?

![Alt Text](https://media.giphy.com/media/vFKqnCdLPNOKc/giphy.gif)



