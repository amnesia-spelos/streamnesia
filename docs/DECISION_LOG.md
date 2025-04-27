# Decision Log

## What is this file?

This is a living document used to track key architectural and design decisions made throughout the development of Streamnesia. By recording decisions here, we avoid revisiting previously rejected ideas, clarify the reasoning behind current patterns, and help onboard new contributors quickly.

---

## [2025-04-26] Amnesia source code extracted to amnesia-tdd-tcp

The decision log about comm protocol and game is going to continue in the [amnesia-tdd-tcp repository](https://github.com/amnesia-spelos/amnesia-tdd-tcp).

---

## [2025-04-23] Payload Auto-Download Defaults to On

**Decision:** Keep the behavior consistent with earlier versions — always download payloads on startup.  
**Why:** Provides a smoother user experience for 3.0 without requiring a more complex update-check system.  
**Revisit:** Post-3.0, when a smarter update strategy can be designed (#42).

---

## [2025-04-22] CommandQueue Will Be Thread-Safe by Design

**Decision:** The new `CommandQueue` must support concurrent additions and non-blocking execution.  
**Why:** Necessary for future multiplayer support and to ensure responsive runtime behavior in the client.

---

## [2025-04-20] TCP Socket Communication Replaces Lock File System

**Decision:** Replace the old lock-file-based IPC with TCP socket communication between the game and the client.  
**Why:** TCP offers better performance, async communication, and is more standard/stable for multi-process interaction. Easier to document and integrate with external tools/languages.
