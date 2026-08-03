# Safe Multi-Platform AssetBundle Builder

## Overview
A Unity Editor tool that safely builds AssetBundles for multiple platforms with customizable bundle names and timer-based platform switching to prevent crashes.

## Features

### 🎯 **Custom Bundle Structure**
- **Android Platform**: 2 bundles (CustomName1 + CustomName2)
- **Desktop Platform**: 1 bundle (CustomName1)
- **WebGL Platform**: 1 bundle (CustomName1)
- **Editable bundle names** for personalization

### 📁 **Organized Output Structure**
```
AssetBundles/
├─ Android/
│   ├─ [CustomName1] (Android scenes)
│   ├─ [CustomName1].manifest
│   ├─ [CustomName2] (Oculus scenes)
│   └─ [CustomName2].manifest
├─ Desktop/
│   ├─ [CustomName1] (Desktop scenes)
│   └─ [CustomName1].manifest
└─ WebGL/
    ├─ [CustomName1] (WebGL scenes)
    └─ [CustomName1].manifest
```

### ⏰ **Safe Platform Switching**
- **Timer-based approach** eliminates compilation waiting issues
- **2-minute safety wait** before WebGL switch
- **3 platform switches total** (Android → Desktop → WebGL)
- **Progress tracking** with real-time countdown

### 🎮 **Scene Management**
- **2 scenes per bundle** (MainMenu + MainScene)
- **Platform-specific scenes** (separate scenes for each platform)
- **Unity Object Picker** for easy scene assignment
- **AssetBundle Browser-like interface**

## How to Use

1. **Open Tool**: `Tools → Safe Multi-Platform Bundle Builder`
2. **Set Bundle Names**: Enter custom names for your bundles
3. **Assign Scenes**: Drag scenes to each platform section
4. **Build**: Click "Build All AssetBundles Safely"
5. **Wait**: Tool handles platform switching and timing automatically

## Build Process
1. **Android**: Builds 2 bundles (main + Oculus scenes)
2. **Desktop**: Switches platform, builds 1 bundle
3. **WebGL**: Waits 2 minutes, switches platform, builds 1 bundle

## Benefits
- ✅ **No crashes** during platform switching
- ✅ **Customizable** bundle names
- ✅ **Efficient** - only 3 platform switches
- ✅ **Organized** folder structure
- ✅ **Safe** timer-based approach
- ✅ **User-friendly** interface

Perfect for developers who need reliable multi-platform AssetBundle building with custom naming and safe platform switching.