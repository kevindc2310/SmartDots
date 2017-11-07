﻿namespace SmartDots.Model
{
    public class DtoSmartdotsSettings
    {
        public bool CanAttachDetachSample { get; set; } //todo make sure this is set to false when working offline
        public bool CanBrowseFolder { get; set; }
        public bool UseSampleStatus { get; set; }
        public bool CanApproveAnnotation { get; set; }
        public bool RequireAq1ForApproval { get; set; }
        public bool RequireParamForApproval { get; set; }
        public bool AutoMeasureScale { get; set; }
        public bool ScanFolder { get; set; }
        public bool AnnotateWithoutSample { get; set; }
        public bool OpenSocket { get; set; }
    }
}
