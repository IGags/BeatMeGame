﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.BeatVertexes
{
    public static class VertexTypeExtensions
    {
        public static bool TryParseVertex(string rawData, out VertexType vertex)
        {
            switch (rawData)
            {
                case "art":
                    vertex = VertexType.Artificial;
                    return true;
                case "bpm":
                    vertex = VertexType.BPM;
                    return true;
                case "fft":
                    vertex = VertexType.FFT;
                    return true;
                case "del":
                    vertex = VertexType.Deletion;
                    return true;
                default:
                    vertex = VertexType.Artificial;
                    return false;
            }
        }
    }
}
