public interface IActivity4Receiver
{
    string Activity4SelectedOptionId { get; }

    string Activity4AudioBase64 { get; }
    byte[] Activity4AudioBytes { get; }

    void SetActivity4SelectedOption(string optionId);
    void ClearActivity4Selection();

    void SetActivity4HasAudio(bool hasAudio);
    void SetActivity4AudioBase64(string base64);
    void SetActivity4AudioBytes(byte[] bytes);

    void ClearActivity4Audio();
}