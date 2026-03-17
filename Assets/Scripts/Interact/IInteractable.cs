/// <summary>
/// Interface cho mọi object có thể tương tác.
/// Interact system (PlayerInteract) chỉ giao tiếp qua interface này,
/// không biết về implementation cụ thể (Dialogue, NPC, Item...).
/// </summary>
public interface IInteractable
{
    /// <summary>Bắt đầu tương tác</summary>
    void OnInteract();

    /// <summary>Tiếp tục tương tác đang diễn ra (ví dụ: next dialogue line)</summary>
    void OnInteractContinue();

    /// <summary>Buộc kết thúc tương tác (ví dụ: player rời xa)</summary>
    void OnInteractEnd();

    /// <summary>Đang trong quá trình tương tác?</summary>
    bool IsInteracting { get; }
}
