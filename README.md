# Jigsaw
The classic jigsaw puzzle game for .Net 10 and Avalonia. 
All ages from 9 to 999 puzzle pieces!

<p align="left"><img src="Screenshots\Screenshot_2026-02-16.png" height="500"/>

Play with any image on your computer! 
Supports dragging and dropping an image file from most web browsers.

<p align="left"><img src="Screenshots\Screenshot 2026-02-16 102558.png" height="500"/>

Very simple, but fun and relaxing. 

Various difficulty levels, piece count, rotations, etc.
Automatically saves your progress, so that you can stop and resume whenever you want.

# Localization

- Human translated: Italian, French and English.

- Machine translated: Spanish, Ukrainian, Bulgarian, Armenian, Greek, German, Japanese, Chinese, Korean, Magyar, Hindi and Bengali.

# Download and play...

Windows x64 build: https://github.com/LaurentInSeattle/Lyt.Jigsaw/blob/main/Download/Jigsaw.zip 
 (Intel CPU 64 bit Only!) 

# Build your own...

- Clone this repo'
- => Clone the "Lyt.Framework" repo' side by side. (https://github.com/LaurentInSeattle/Lyt.Framework)
- => Clone the "Lyt.Avalonia" repo' side by side. (https://github.com/LaurentInSeattle/Lyt.Avalonia)
- Open the solution in Visual Studio, restore nugets, then clean and build.

Developed and tested with .Net 10, Visual Studio 2026 18.3 and Avalonia 11.3.12.
Also builds with Jet Brains Rider. On Mac and Linux, you should disable the post build event, as it is Windows specific.

# Dependencies

- Avalonia (Skia)
- Microsoft Dependency Injection and Hosting Framework
- Microsoft Community Toolkit MVVM Framework
