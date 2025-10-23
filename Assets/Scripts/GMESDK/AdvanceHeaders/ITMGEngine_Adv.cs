using System;
using System.Runtime.InteropServices;

namespace GME
{
    public enum ITMGRecordingType
    {
        ITMG_AUDIO_RECORDING_SELF = 0,//Record your own voice,including accompaniment and vocal
        ITMG_AUDIO_RECORDING_KTV = 1,// Record songs of karaoke mode
    };

    public abstract class ITMGAudioRecordCtrl
    {
        public static ITMGAudioRecordCtrl GetInstance()
        {
            return QAVAudioRecordCtrl.GetInstance();
        }
        public abstract event QAVAudioRecordCallback OnAudioRecordComplete;
        public abstract event QAVAudioRecordPreviewCallback OnAudioRecordPreviewComplete;
        public abstract event QAVAudioRecordMixCallback OnAudioRecordMixComplete;

        public abstract int StartRecord(ITMGRecordingType type, string dstFile, string accMixFile, string accPlayFile);
        public abstract int StopRecord();
        public abstract int PauseRecord();
        public abstract int ResumeRecord();
        public abstract int SetAccompanyFile(string accMixFile);
        public abstract int GetAccompanyTotalTimeByMs();
        public abstract int GetRecordTimeByMs();
        public abstract int SetRecordTimeByMs(int timePlay, int timeRecord);
        public abstract int SetRecordKaraokeType(int type);
        public abstract int GetRecordFileDurationByMs();

        public abstract int StartPreview();
        public abstract int StopPreview();
        public abstract int PausePreview();
        public abstract int ResumePreview();
        public abstract int SetPreviewTimeByMs(int time);
        public abstract int GetPreviewTimeByMs();

        public abstract int SetMixWeights(float mic, float acc);
        public abstract int AdjustAudioTimeByMs(int time);
        public abstract int MixRecordFile(bool needMicData);
        public abstract int CancelMixRecordFile();
        public abstract int CleanTask();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GMECustomStreamFrame
    {
        public UInt64 uin;
        public IntPtr data;
        public uint length;
        public UInt64 timestamp;
    }

    public enum Audio_Data_Type{
        AUDIO_DATA_TYPE_CAPTURE,
        AUDIO_DATA_TYPE_LOOPBACK,
        AUDIO_DATA_TYPE_SEND,
        AUDIO_DATA_TYPE_PLAY,
        AUDIO_DATA_TYPE_CAPTURE_PLAY,
        AUDIO_DATA_TYPE_MIX_TO_PLAY,
        AUDIO_DATA_TYPE_CAPTURE_DISPOSE,
        AUDIO_DATA_TYPE_COUNT
    };

    public abstract class ITMGAudioDataObserver
    {
        public abstract event QAVAudioDataCallback OnAudioDataCallback;
        public static ITMGAudioDataObserver GetInstance()
        {
            return QAVAudioDataObserver.GetInstance();
        }

        public abstract int RegisteAudioDataCallback(Audio_Data_Type dataType);

        public abstract int UnRegisteAudioDataCallback(Audio_Data_Type dataType);

        public abstract int SetAudioDataFormat(Audio_Data_Type audioType, int sampleRate, int channelCount);
    }

}