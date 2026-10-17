using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ImageGalleryApp.Models;

namespace ImageGalleryApp.Views
{
    public partial class EliminationWindow : Window
    {
        private List<ImageItem> _candidates;
        private List<ImageItem> _currentRound;
        private List<ImageItem> _nextRound;
        private int _currentPairIndex;
        private readonly int _targetCount;

        public List<ImageItem> Winners { get; private set; } = new();

        public EliminationWindow(List<ImageItem> images, int targetCount)
        {
            InitializeComponent();
            _candidates = new List<ImageItem>(images);
            _targetCount = targetCount;
            StartElimination();
        }

        private void StartElimination()
        {
            _currentRound = new List<ImageItem>(_candidates);
            _nextRound = new List<ImageItem>();
            _currentPairIndex = 0;

            ShowNextPair();
        }

        private void ShowNextPair()
        {
            if (_currentRound.Count <= _targetCount)
            {
                // We've reached our target
                Winners = _currentRound;
                DialogResult = true;
                Close();
                return;
            }

            if (_currentPairIndex >= _currentRound.Count)
            {
                // Round complete, start next round
                if (_currentRound.Count % 2 == 1)
                {
                    // Odd number, automatically advance the last one
                    _nextRound.Add(_currentRound.Last());
                }

                _currentRound = _nextRound;
                _nextRound = new List<ImageItem>();
                _currentPairIndex = 0;

                ShowNextPair();
                return;
            }

            if (_currentPairIndex + 1 >= _currentRound.Count)
            {
                // Only one image left in this round
                _nextRound.Add(_currentRound[_currentPairIndex]);
                _currentPairIndex += 2;
                ShowNextPair();
                return;
            }

            // Show current pair
            var leftImage = _currentRound[_currentPairIndex];
            var rightImage = _currentRound[_currentPairIndex + 1];

            LeftImage.Source = leftImage.Thumbnail;
            RightImage.Source = rightImage.Thumbnail;

            UpdateProgress();
        }

        private void UpdateProgress()
        {
            var totalPairs = (_currentRound.Count + 1) / 2;
            var currentPair = (_currentPairIndex / 2) + 1;
            var remaining = _currentRound.Count;

            ProgressText.Text = $"Round Progress: {currentPair}/{totalPairs} | Remaining: {remaining} → Target: {_targetCount}";
        }

        private void LeftImageButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseImage(_currentRound[_currentPairIndex]);
        }

        private void RightImageButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseImage(_currentRound[_currentPairIndex + 1]);
        }

        private void ChooseImage(ImageItem chosen)
        {
            _nextRound.Add(chosen);
            _currentPairIndex += 2;
            ShowNextPair();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}