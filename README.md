# QR Canvas Mapper

## Description
QR Canvas Mapper is a module for mapping QR Codes in the ScreenSpace to objects in the game.

## Modules

### QRObjectHandler
Responsible for mapping QRCode positions and angles from input text (kinect or webcam) to a selected array of game objects rotation and array. It also keeps into account the tracking of multiple QR Codes and maps them accordingly to specified index in the array of game objects.

### CalibrateScript
Is a W.I.P script for calibrating this setup from the Kinect with the casted projection specific for the active-floor setup. But could be used for any setup where a projection does not line exactly up with the worldview of the kinect's RGB camera.

## Dependencies
ZXing - [GitHub](https://github.com/micjahn/ZXing.Net)