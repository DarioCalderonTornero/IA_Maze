# 🧭 Unity Project - Pathfinding with A* (FindPathAStar)

## 📌 Overview

This project implements a pathfinding system in Unity using the **A\*** (A Star) algorithm. The main goal is to find the most optimal path between two points within a grid-based environment.

Unlike simpler approaches, this system doesn’t just move through the map — it continuously evaluates which step is best based on both the accumulated cost and the estimated distance to the goal.

The core of the project is centered around the **FindPathAStar** class, where all the algorithm logic and its visual representation are handled.

---

## 🧠 What makes FindPathAStar special?

This implementation doesn’t just compute a path — it **visualizes the entire process step by step**, making it easier to understand how the algorithm explores the space.

Each evaluated tile becomes a node containing:

- Its position in the map  
- The accumulated cost from the start (g)  
- The estimated cost to the goal (h)  
- The total cost (f = g + h)  
- A reference to its parent node  

This structure allows reconstructing the final path once the destination is reached.

📄 Main script: :contentReference[oaicite:0]{index=0}

---

## 🔍 How the algorithm works

### 1. Initialization

The algorithm starts by selecting two valid positions on the map:

- **Start node**
- **Goal node**

Then, two key structures are initialized:

- **OPEN** → nodes to be explored  
- **CLOSED** → nodes already evaluated  

The start node is added to CLOSED, and the search begins from there.

---

### 2. Grid-based exploration

The system navigates the map by evaluating **neighboring tiles in 4 directions**:

- Up  
- Down  
- Left  
- Right  

These directions are defined within the map coordinate system, allowing the environment to be treated as a grid :contentReference[oaicite:1]{index=1}

For each neighbor:

- Invalid tiles are discarded (walls or out of bounds)  
- Already visited nodes (in CLOSED) are ignored  
- Their **g, h, and f values** are calculated  

---

### 3. Node evaluation

Each generated node is handled as follows:

- If it already exists in OPEN → update it if a better path is found  
- If it doesn’t exist → add it to OPEN  

This ensures the algorithm always keeps track of the best possible options.

---

### 4. Choosing the next step

At each iteration, the algorithm:

- Selects the node in OPEN with the lowest **f(n)**  
- Moves it to CLOSED  
- Continues the search from that node  

This behavior makes the algorithm always advance toward the most promising path.

---

### 5. Completion

The algorithm finishes when:

- The goal node is reached  
- Or there are no more nodes to explore  

Once the goal is reached, the path is reconstructed by following parent nodes from the end back to the start.

---

## 🎮 Real-time interaction

The system allows controlling the algorithm execution through keyboard input:

- **P** → Start the search  
- **C** → Step forward manually  
- **A** → Run automatically (animation)  
- **M** → Display the final path  

This turns the project into a very visual and interactive way to understand A* in action.

---

## 🧱 Visual representation

Each node is represented as an object in the scene:

- Nodes in OPEN → one color  
- Nodes in CLOSED → another color  
- Final path → clean and highlighted  

Additionally, each tile displays:

- g(n)  
- h(n)  
- f(n)  

This makes it easy to see how decisions are made during execution.

---

## 🧩 System structure

The project is built around several key components:

- **FindPathAStar** → algorithm logic  
- **PathMarker** → node representation  
- **MapLocation** → coordinate system  
- **Maze** → navigation environment  

---

## 🚀 Conclusion

This project doesn’t just implement A* — it turns it into a visual and interactive experience. It helps to understand:

- How nodes are expanded  
- How costs are calculated  
- How the optimal path is selected  

In short, it’s a great tool for learning and visualizing how the A* algorithm works inside a Unity environment.

