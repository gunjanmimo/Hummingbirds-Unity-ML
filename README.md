# **Hummingbirds Unity Machine Learning Agent**
Hummingbird Unity machine learning agent is the artificial intelligent system where I tried to copy the Hummingbird flying nature in Unity game environment and train agents that can navigate to flowers, dip their beaks in, and drink nectar. These hummingbirds have six degrees of freedom, meaning they can fly and turn in any direction to find targets.

These hummingbirds have six degrees of freedom, meaning they can fly and turn in any direction to find targets. They have more complicated controls and their flight paths cannot be solved with traditional navigation systems. In this project I proposed a Reinforcement Learning model with newly Invented [Unity Machine Learning ](https://github.com/Unity-Technologies/ml-agents). 

## **Environment:**
![](https://github.com/gunjanmimo/Hummingbirds-Unity-ML/blob/main/img/image12.png?raw=true)
<center><em>fig 1: Unity Environment</em></center>

The hummingbird agent is the main machine learning agent and the simulation environment has different kind of  obstacles for the agent. 

## **Control:**

Direction | Key     |Direction | Key     
----------|---------|----------|---------
Forward   | `W`     |Pitch Up  | `↑`
Backward  | `S`     |Pitch Down| `↓`
Left      | `A`     |Turn Left | `←`
Right     | `D`     |Turn Right| `→`
Up        | `E`     |
Down      | `C`     |


![](https://www.matrixtsl.com/wikiv6/images/7/76/Eg_Pitch_Yaw_Roll.svg)

<center><em>fig 2: pitch and yaw movement of flying object</em></center>


## **Game Mode:**
To Control the agent using keyboard, in **Behavior Parameter** change the **Behavior Type** of agent to **Heuristic**

## **Training:**


In `Behavior Parameter` Change the `Behavior Type` of `Hummingbird` to `Default`.
### **1. Running training**

`mlagents-learn ./config/trainer_config.yaml --run-id trainingAgent`

### **2. Visualization**
use `tensorboard --logdir ./config/summaries`

![](https://github.com/gunjanmimo/Hummingbirds-Unity-ML/blob/main/img/image3.png?raw=true)

<center><em>fig 3: Shows the training progression on the four games using ML-Agents in the Unity environment. Mean cumulative training reward across eight game instances PPO (8) orange. The x-axis denotes steps. Note that rewards and the y-axis are different for each game. The results clearly show learning progress across all eight games, in line with the baselines.</em></center>



![](https://github.com/gunjanmimo/Hummingbirds-Unity-ML/blob/main/img/image14.png?raw=true)

<center><em>fig 4: Shows that the Policy/Entropy curve is decreasing which means taking random decisions of the model slowly decrease during a successful training process. Policy/Extrinsic Reward, the mean cumulative reward received from the environment per-episode increases with training. Policy/Learning Rate, how large a step the training algorithm takes as it searches for the optimal policy  is decreasing with time being.</em></center>



### **3. Using Trained brain in Machine Learning Agent**

Use the `Hummingbird.nn` file in the directory specified in `--run-id` parameter during training as `Model` for `Hummingbird`'s `Behavior Parameter`. This `Hummingbird` is the Machine Learning agent against which the player will compete. For the Player `Hummingbird`, set `Behavior Type` to `Heuristic Only`.

# **Final result:**
<center><img src="https://github.com/gunjanmimo/Hummingbirds-Unity-ML/blob/main/img/video.gif?raw=true" /></center>
