## 📜 Overview
A modular Unity UI system using **UI Toolkit** with reusable screens, dynamic activation, runtime/editor configuration, localization, and idle animations.

---

## 🧭 Main Scenes
- **HomeScreen** – Entry + bottom bar navigation.
- **SettingsPopup** – Localizable popup.
- **LevelCompletedScreen** – Celebration screen with custom animations.

---

## 🛠 Tech Details
- **Unity Version:** Latest available version
- **Packages:** UI Toolkit, DOTween.
- **Start Scene:** `HomeScreen`

---

## 🎯 Task Highlights
1. **Home Screen** – Responsive background, nav buttons (locked/unlocked), appear/disappear animations, event‑driven.
2. **Settings Popup** – Modular prefab, localizable text.
3. **Level Completed** – Triggered from Home, custom animations.

---

## 🧩 Architecture
- `ScreenManager` auto‑detects screens, uses USS transitions.
- `ScreenManagerEditor` for easy button‑to‑screen mapping.
- `IdleAnimator` supports UGUI + UI Toolkit.
- USS for styling and transitions; scripts focus on logic.

---

## 🌍 Localization
- Dictionary‑based, supports runtime language switching.
- Recursive element detection for complete coverage.

---

## 🚀 Run Instructions
```bash
git clone [REPO_URL]
