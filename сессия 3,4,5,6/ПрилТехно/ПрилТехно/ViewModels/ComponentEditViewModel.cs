using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ПрилТехно.Models;

namespace ПрилТехно.ViewModels
{
    public partial class ComponentEditViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RawMaterialDto> _rawMaterials = new();

        [ObservableProperty]
        private RawMaterialDto? _selectedRawMaterial;

        [ObservableProperty]
        private decimal _percentage;

        [ObservableProperty]
        private int _loadOrder;

        [ObservableProperty]
        private decimal? _toleranceMin;

        [ObservableProperty]
        private decimal? _toleranceMax;

        // Результирующий компонент
        private RecipeComponentDto? _component;
        public RecipeComponentDto? Component
        {
            get => _component;
            set => SetProperty(ref _component, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event EventHandler? CloseRequest;

        public ComponentEditViewModel(List<RawMaterialDto> rawMaterials, RecipeComponentDto? existing = null)
        {
            RawMaterials = new ObservableCollection<RawMaterialDto>(rawMaterials);
            if (existing != null)
            {
                // Режим редактирования
                Component = existing;
                SelectedRawMaterial = RawMaterials.FirstOrDefault(r => r.Id == existing.RawMaterialId);
                Percentage = existing.Percentage;
                LoadOrder = existing.LoadOrder;
                ToleranceMin = existing.ToleranceMin;
                ToleranceMax = existing.ToleranceMax;
            }
            else
            {
                // Новый компонент
                Component = new RecipeComponentDto();
                Percentage = 0;
                LoadOrder = 0;
            }

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
        }

        private void Save()
        {
            if (SelectedRawMaterial == null)
            {
                MessageBox.Show("Выберите сырьё", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Percentage <= 0 || Percentage > 100)
            {
                MessageBox.Show("Доля должна быть от 0 до 100", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Component!.RawMaterialId = SelectedRawMaterial.Id;
            Component.RawMaterialName = SelectedRawMaterial.Name;
            Component.Percentage = Percentage;
            Component.LoadOrder = LoadOrder;
            Component.ToleranceMin = ToleranceMin;
            Component.ToleranceMax = ToleranceMax;
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}