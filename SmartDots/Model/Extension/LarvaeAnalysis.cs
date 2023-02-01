﻿using System;
using System.Collections.Generic;

namespace SmartDots.Model
{
    [Serializable]
    public class LarvaeAnalysis
    {
        public Guid ID { get; set; }
        public string HeaderInfo { get; set; }
        public string Type { get; set; }
        public bool AllowSetScale { get; set; }
        public List<LarvaeSample> LarvaeSamples { get; set; } = new List<LarvaeSample>();
        public List<LarvaeParameter> LarvaeParameters { get; set; } = new List<LarvaeParameter>();


    }
}
