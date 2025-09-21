using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace game_mono
{
    /// <summary>
    /// Renders a ground plane with grid lines to provide visual reference for 3D navigation
    /// </summary>
    public class GroundPlane
    {
        private int _gridSize;
        private float _gridSpacing;
        private Color _primaryColor;
        private Color _secondaryColor;
        
        public GroundPlane(int gridSize = 50, float gridSpacing = 2f, Color? primaryColor = null, Color? secondaryColor = null)
        {
            _gridSize = gridSize;
            _gridSpacing = gridSpacing;
            _primaryColor = primaryColor ?? new Color(100, 100, 100, 255); // Dark gray
            _secondaryColor = secondaryColor ?? new Color(60, 60, 60, 255); // Darker gray
        }
        
        /// <summary>
        /// Draws the ground plane using the provided renderer
        /// </summary>
        public void Draw(PrimitiveRenderer renderer)
        {
            if (renderer == null) return;
            
            float halfSize = (_gridSize * _gridSpacing) * 0.5f;
            
            // Generate horizontal lines (running along X-axis)
            for (int z = 0; z <= _gridSize; z++)
            {
                float zPos = (z * _gridSpacing) - halfSize;
                Color lineColor = (z % 10 == 0) ? _primaryColor : _secondaryColor; // Every 10th line is brighter
                
                Vector3 startPos = new Vector3(-halfSize, 0, zPos);
                Vector3 endPos = new Vector3(halfSize, 0, zPos);
                
                renderer.AddLine(startPos, endPos, lineColor, 0.02f);
            }
            
            // Generate vertical lines (running along Z-axis)
            for (int x = 0; x <= _gridSize; x++)
            {
                float xPos = (x * _gridSpacing) - halfSize;
                Color lineColor = (x % 10 == 0) ? _primaryColor : _secondaryColor; // Every 10th line is brighter
                
                Vector3 startPos = new Vector3(xPos, 0, -halfSize);
                Vector3 endPos = new Vector3(xPos, 0, halfSize);
                
                renderer.AddLine(startPos, endPos, lineColor, 0.02f);
            }
            
            // Add center axes with different colors for orientation
            // X-axis (red) - left to right
            renderer.AddLine(
                new Vector3(-halfSize, 0.01f, 0), 
                new Vector3(halfSize, 0.01f, 0), 
                Color.Red, 0.05f);
            
            // Z-axis (blue) - forward to back
            renderer.AddLine(
                new Vector3(0, 0.01f, -halfSize), 
                new Vector3(0, 0.01f, halfSize), 
                Color.Blue, 0.05f);
            
            // Y-axis (green) - up from center
            renderer.AddLine(
                new Vector3(0, 0, 0), 
                new Vector3(0, 2f, 0), 
                Color.Green, 0.05f);
        }
        
        /// <summary>
        /// Updates grid colors for animation effects (optional)
        /// </summary>
        public void UpdateColors(Color primaryColor, Color secondaryColor)
        {
            _primaryColor = primaryColor;
            _secondaryColor = secondaryColor;
        }
    }
}