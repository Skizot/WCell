﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps
{
    /// <summary>
    /// MDDF Chunk Class - Placement information for M2 Models on map tiles
    /// </summary>
    public class MapDoodadDefinition
    {
        /// <summary>
        /// Filename of the M2
        /// </summary>
        public string FilePath;

        /// <summary>
        /// Unique ID of the M2 in this ADT
        /// </summary>
        public uint UniqueId;

        /// <summary>
        /// Position of the M2
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Rotation around the Z axis
        /// </summary>
        public float OrientationA;

        /// <summary>
        /// Rotation around the Y axis
        /// </summary>
        public float OrientationB;

        /// <summary>
        /// Rotation around the X axis
        /// </summary>
        public float OrientationC;

        /// <summary>
        ///  Scale factor of the M2
        /// </summary>
        public float Scale;
    }
}
