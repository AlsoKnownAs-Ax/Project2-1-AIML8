# ML Model Performance Analysis

## Overview

This analysis compares different Memory Sensor model configurations against the base case, examining different level of steps.

## Configurations

- Base Case (Default)
- Memory Sensor (50 million steps)
- Memory Sensor (20 million setps)

## Metrics Analysis

### ELO Rating

![ELO Rating Comparison](ELO_comparison.png)

| Configuration         | Mean    | Std Dev | Min     | Max     |
| --------------------- | ------- | ------- | ------- | ------- |
| Base Case             | 1506.60 | 66.53   | 1199.95 | 1608.24 |
| Memory Sensor 50      | 1522.45 | 53.22   | 1194.28 | 1601.35 |
| Memory Sensor 20      | 1280.55 | 109.63  | 1167.39 | 1459.09 |



### Entropy

![Entropy Comparison](Entropy_comparison.png)

| Configuration         | Mean | Std Dev | Min  | Max  |
| --------------------- | ---- | ------- | ---- | ---- |
| Base Case             | 1.57 | 0.34    | 1.26 | 3.29 |
| Memory Sensor 50      | 1.68 | 0.31    | 1.37 | 3.28 |
| Memory Sensor 20      | 2.81 | 0.49    | 1.85 | 3.28 |


### Group Cumulative Reward

![Group Cumulative Reward Comparison](Group_Cumulative_reward_comparison.png)

| Configuration         | Mean   | Std Dev | Min    | Max   |
| --------------------- | ------ | ------- | ------ | ----- |
| Base Case             | -0.019 | 0.098   | -0.359 | 0.293 |
| Memory Sensor 50      | -0.026 | 0.095   | -0.590 | 0.307 |
| Memory Sensor 20      | -0.036 | 0.137   | -0.615 | 0.380 |

### Policy Loss

![Policy Loss Comparison](Policy_Loss_comparison.png)

| Configuration         | Mean  | Std Dev | Min   | Max   |
| --------------------- | ----- | ------- | ----- | ----- |
| Base Case             | 0.017 | 0.002   | 0.010 | 0.027 |
| Memory Sensor 50      | 0.017 | 0.002   | 0.011 | 0.028  |
| Memory Sensor 20      | 0.017 | 0.002   | 0.011 | 0.026 |

### Value Loss

![Value Loss Comparison](Value_Loss_comparison.png)

| Configuration         | Mean  | Std Dev | Min   | Max   |
| --------------------- | ----- | ------- | ----- | ----- |
| Base Case             | 0.130 | 0.026   | 0.000 | 0.160 |
| Memory Sensor 50      | 0.134 | 0.022   | 0.000 | 0.165 |
| Memory Sensor 20      | 0.044 | 0.058   | 0.000 | 0.150 |

## Key Observations

0. #### Goals Scored
Fixed Delta Time: 0.01

Memory Based Sensor 50 million: 105574 goals
Base Case: 111967 goals

1. #### ELO
   - Description
   - **Evaluation**: 
2. #### Entropy
   - Description
   - **Evaluation**: 
3. #### Group Cumulative Rewards
   - Description
   - **Evaluation**: 
4. #### Policy Loss
   - Description
   - **Evaluation**: 
5. #### Value Loss
   - Description
   - **Evaluation**: 