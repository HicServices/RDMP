using System.Windows;
using System.Windows.Media;
using GraphX.Controls;
using GraphX.PCL.Common.Enums;
using QuickGraph;

namespace ReusableUIComponents.Dependencies.Models
{
    /// <summary>
    /// This is custom GraphArea representation using custom data types.
    /// GraphArea is the visual panel component responsible for drawing visuals (vertices and edges).
    /// It is also provides many global preferences and methods that makes GraphX so customizable and user-friendly.
    /// </summary>
    public class GraphAreaExample : GraphArea<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>
    {
         
    }
}
