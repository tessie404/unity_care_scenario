# Unity Care Scenario Demo
## Overview
The system simulates a night-shift care environment where multiple events occur simultaneously, requiring real-time prioritization.
It is designed to capture how different decision strategies emerge under constraints, rather than solving for a single optimal outcome.

## System Architecture
The system is built as a modular simulation framework:
- Task manager for scheduling and triggering events
- UI layer for user interaction and decision input
- State tracking for task status and dynamic risk levels
- Logging module for recording user actions and timestamps
This structure makes it possible to extend the system toward data-driven analysis or more complex behavioral modeling.

## Task Design
The system presents multiple concurrent tasks (A, B, C), each with different urgency and severity levels.
This allows controlled manipulation of:
- task difficulty
- prioritization pressure
- decision complexity

## Interaction Design
Each task includes optional hover-based explanations, allowing users to access additional information before making decisions.
This design makes it possible to separate:
- information processing time (reading / understanding)
- decision-making time
which is useful when analyzing user behavior under time pressure.

## Research Goal
The focus of this project is to understand how people make trade-offs under competing demands.
Instead of assuming a single optimal solution, the system models a **decision strategy space**, capturing trade-offs.
For example:
- efficiency vs emotional care
- speed vs completeness
- different prioritization strategies across users

## Data & Evaluation
The system collects behavioral data such as:
- task selection order
- decision latency
- interaction patterns

To better understand decision strategies, the system considers both objective and subjective measures:
- Objective metrics: task completion, risk reduction, decision latency  
- Subjective feedback: perceived stress, perceived appropriateness of decisions, emotional considerations  
Combining these perspectives helps characterize different decision strategies, which could inform adaptive AI systems that provide user-aligned recommendations.

## Human–AI Interaction Extensions
Future work will introduce human-AI collaboration, such as:
- AI-assisted prioritization
- adaptive suggestions based on user behavior
- comparison between human-only and human-AI decisions strategies

## Future Work
- integrating emotional modeling (e.g., anxiety, irritation, self-blame)
- simulating more realistic behavioral responses
- extending toward data-driven evaluation of decision strategies

## Tech Stack
- Unity
- C#
