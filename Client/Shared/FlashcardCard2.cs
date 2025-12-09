using System.Collections.Generic;

namespace studbud.Client.Shared
{
    public class FlashcardCard2
    {
        public string front { get; set; }
        public string back { get; set; }
    }

    public class FlashcardStateService
    {
        public List<FlashcardCard> Flashcards { get; private set; } = new List<FlashcardCard>();

        public event Action OnChange;

        public void SetFlashcards(List<FlashcardCard> newFlashcards)
        {
            Flashcards = newFlashcards;
            NotifyStateChanged();
        }

        public void ClearFlashcards()
        {
            Flashcards.Clear();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
