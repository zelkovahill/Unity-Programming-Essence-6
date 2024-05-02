#!/bin/bash

# Path to Unity Editor executable
UNITY_EDITOR_PATH="/Applications/Unity/Hub/Editor/6000.0.0b16/Unity.app/Contents/MacOS/Unity"

# Ask the user for the folder containing Unity projects
read -p "Enter the path to the folder containing Unity projects: " PROJECTS_PATH

# Check if the path exists
if [ ! -d "$PROJECTS_PATH" ]; then
   echo "The specified folder does not exist."
   exit 1
fi

# Function to check if a given directory is a Unity project
function is_unity_project() {
  if [ -d "$1/Assets" ] && [ -d "$1/ProjectSettings" ]; then
    return 0 # True, it is a Unity project
  else
    return 1 # False, it is not a Unity project
  fi
}

# Get a list of all directories within the specified folder, recursively
PROJECTS=($(find "$PROJECTS_PATH" -type d))

# Only keep Unity projects
UNITY_PROJECTS=()
for project in "${PROJECTS[@]}"; do
  if is_unity_project "$project"; then
    UNITY_PROJECTS+=("$project")
  fi
done

echo "Found ${#UNITY_PROJECTS[@]} Unity projects."

# Counter for the progress display
COUNT=0
TOTAL_COUNT=${#UNITY_PROJECTS[@]}

# Loop through each Unity project directory
for UNITY_PROJECT_DIR in "${UNITY_PROJECTS[@]}"; do
  ((COUNT++))
  echo "Processing project $COUNT/$TOTAL_COUNT: $UNITY_PROJECT_DIR"

  # Create Editor directory if it does not exist
  mkdir -p "$UNITY_PROJECT_DIR/Assets/Editor"

  # Copy the ForceReserializeAssets.cs script to the project's "Assets/Editor/" directory
  cp "$PROJECTS_PATH/ForceReserializeAssets.cs" "$UNITY_PROJECT_DIR/Assets/Editor/ForceReserializeAssets.cs"

  # Run Unity in batch mode and execute ForceReserializeAssets.Reserialize method
  "$UNITY_EDITOR_PATH" -batchmode -quit -executeMethod ForceReserializeAssets.Reserialize -projectPath "$UNITY_PROJECT_DIR"
  
  # Show progress
  PERCENT=$((COUNT * 100 / TOTAL_COUNT))
  echo "Progress: $PERCENT%"
    
  # Provide a little pause between projects
  sleep 10

done

echo "Completed processing Unity projects."
