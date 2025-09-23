# My project

A Unity project. This repository includes only the files needed for source control; transient build and cache folders are ignored.

## Requirements
- Unity: 6000.1.14f1
- Optional: Git LFS for large/binary assets (models, textures, audio)

## Getting Started
- Open Unity Hub, add this folder, and open with Unity 6000.1.14f1.
- Recommended: In Unity, set Asset Serialization to Force Text and enable Visible Meta Files (Edit -> Project Settings -> Editor). This improves diffs and merge behavior.

## Build
- Use File -> Build Settings to create builds for your target platform(s). Build outputs are ignored by Git.

## Version Control Notes
- Tracked: `Assets/`, `Packages/`, `ProjectSettings/` (and their `.meta` files).
- Ignored: Unity caches (`Library/`, `Temp/`, `Obj/`), IDE files, and build artifacts. See `.gitignore` for details.

### Optional: Git LFS (recommended)
Large assets benefit from LFS to keep the repo lean.

```bash
# One-time, from the repo root
git lfs install
# Common Unity asset types
git lfs track "*.psd" "*.tga" "*.tif" "*.png" "*.jpg" "*.jpeg" \
               "*.wav" "*.mp3" "*.ogg" "*.fbx" "*.obj" "*.prefab" \
               "*.anim" "*.controller" "*.mp4" "*.mov"
# After tracking, commit the updated .gitattributes
```

## First Push
If this folder isn’t a Git repo yet:

```bash
git init
git add .
git commit -m "Initial Unity project"
# Replace URL with your remote (GitHub, GitLab, etc.)
git branch -M main
git remote add origin <your-remote-url>
git push -u origin main
```

---
Notes:
- This README was generated from the project’s `ProjectSettings/ProjectVersion.txt` to pin the Unity version.
- Consider setting up Unity Smart Merge (UnityYAMLMerge) for better merges of `.unity` and `.prefab` files.
