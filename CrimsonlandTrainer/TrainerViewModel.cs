using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrimsonlandTrainer.Game;
using CrimsonlandTrainer.Utility;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;

namespace CrimsonlandTrainer {
    public class TrainerViewModel : BindableBase {
        private readonly TrainerModel _model = new TrainerModel();
        private bool _isUpdatingFromModel = false;
        private bool _replacePlasmaOverloadWith500Points;
        private bool _replaceFireBulletsWith500Points;

        public DelegateCommand ApplicationStartCommand { get; }
        public DelegateCommand ApplicationCloseCommand { get; }
        public DelegateCommand StartGameCommand { get; }
        public DelegateCommand LoadBuildCommand { get; }
        public DelegateCommand SaveBuildCommand { get; }
        public DelegateCommand ResetBuildStateCommand { get; }

        public string StatusText {
            get {
                switch (_model.Status) {
                    case TrainerStatus.GameProcessNotFound:
                        return "Crimsonland.exe process not detected";
                        break;
                    case TrainerStatus.WaitingForGameModule:
                        return "Waiting for Crimsonland to load...";
                        break;
                    case TrainerStatus.AttachedToGameProcess:
                        return "Attached to the game!";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public string DebugEngineOutput => _model.DebugEngineOutput;

        public bool IsLoadedBuildGroupBoxEnabled => true;
        public bool IsStartGameButtonEnabled => _model.Status == TrainerStatus.GameProcessNotFound;

        public DelegateCommand<Perk?> AddPerkToBuildCommand { get; }
        public DelegateCommand<int?> DeletePerkFromBuildCommand { get; }
        public DelegateCommand<Weapon?> AddWeaponToBuildCommand { get; }
        public DelegateCommand<int?> DeleteWeaponFromBuildCommand { get; }

        public ObservableCollection<PerkViewModel> Perks { get; set; } = new ObservableCollection<PerkViewModel>();
        public ObservableCollection<WeaponViewModel> Weapons { get; set; } = new ObservableCollection<WeaponViewModel>();

        public bool ReplaceFireBulletsWith500Points {
            get => _replaceFireBulletsWith500Points;
            set {
                _replaceFireBulletsWith500Points = value;
                _model.BuildModel.Settings.ReplaceFireBulletsWith500Points = value;
            }
        }

        public bool ReplacePlasmaOverloadWith500Points {
            get => _replacePlasmaOverloadWith500Points;
            set {
                _replacePlasmaOverloadWith500Points = value;
                _model.BuildModel.Settings.ReplacePlasmaOverloadWith500Points = value;
            }
        }

        public TrainerViewModel() {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Formatting = Formatting.Indented;
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            _model.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(TrainerModel.Status):
                        RaisePropertyChanged(nameof(StatusText));
                        break;
                    case nameof(TrainerModel.DebugEngineOutput):
                        RaisePropertyChanged(nameof(DebugEngineOutput));
                        break;
                    case nameof(TrainerModel.CurrentPerkIndex):
                    case nameof(TrainerModel.CurrentWeaponIndex):
                        Validate();
                        break;
                    default:
                        RaisePropertyChanged(e.PropertyName);
                        break;
                }
            };

            ApplicationStartCommand = new DelegateCommand(() => _model.Start());
            ApplicationCloseCommand = new DelegateCommand(() => _model.Stop());
            StartGameCommand = new DelegateCommand(() => {
                Process.Start("steam://run/262830");
            });
            LoadBuildCommand = new DelegateCommand(() => {
                OpenFileDialog openFileDialog = new OpenFileDialog {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    Filter = "Crimsonland Build (*.json)|*.json"
                };
                if (openFileDialog.ShowDialog() == true) {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    BuildModel buildModel = JsonConvert.DeserializeObject<BuildModel>(json, jsonSerializerSettings);
                    _model.BuildModel = buildModel;
                    UpdateFromModel(_model.BuildModel);
                    Validate();
                }
            });

            SaveBuildCommand = new DelegateCommand(() => {
                SaveFileDialog saveFileDialog = new SaveFileDialog {
                    CheckPathExists = true,
                    InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    Filter = "Crimsonland Build (*.json)|*.json"
                };
                if (saveFileDialog.ShowDialog() == true) {
                    string json = JsonConvert.SerializeObject(_model.BuildModel, jsonSerializerSettings);
                    File.WriteAllText(saveFileDialog.FileName, json);
                }
            });

            Perks.CollectionChanged += (sender, args) => {
                if (_isUpdatingFromModel)
                    return;

                Validate();

                _model.BuildModel.Perks.Clear();
                _model.BuildModel.Perks.AddRange(Perks.Select(vm => vm.Perk));
            };

            Weapons.CollectionChanged += (sender, args) => {
                if (_isUpdatingFromModel)
                    return;

                _model.BuildModel.Weapons.Clear();
                _model.BuildModel.Weapons.AddRange(Weapons.Select(vm => vm.Weapon));
            };

            AddPerkToBuildCommand = new DelegateCommand<Perk?>(perk => {
                Perks.Add(new PerkViewModel(perk.Value));
            });

            DeletePerkFromBuildCommand = new DelegateCommand<int?>(index => {
                if (index == -1)
                    return;

                Perks.RemoveAt(index.Value);
            });

            AddWeaponToBuildCommand = new DelegateCommand<Weapon?>(weapon => {
                Weapons.Add(new WeaponViewModel(weapon.Value));
            });

            DeleteWeaponFromBuildCommand = new DelegateCommand<int?>(index => {
                if (index == -1)
                    return;

                Weapons.RemoveAt(index.Value);
            });

            ResetBuildStateCommand = new DelegateCommand(() => {
                _model.ResetState();
            });
        }

        private void Validate() {
            (int errorPerkIndex, string errorText)[] perkValidationErrors =
                PerkListValidator.Validate(Perks.Select(vm => vm.Perk).ToArray());

            for (int i = 0; i < Perks.Count; i++) {
                PerkViewModel perkViewModel = Perks[i];

                int indexCopy = i;
                string perkValidationError =
                    String.Join(
                        "\r\n",
                        perkValidationErrors
                            .Where(tuple => tuple.errorPerkIndex == indexCopy)
                            .Select(tuple => tuple.errorText));

                perkViewModel.Error = perkValidationErrors.Length == 0 ? null : perkValidationError;
                perkViewModel.IsNext = i == _model.CurrentPerkIndex;
            }

            for (int i = 0; i < Weapons.Count; i++) {
                WeaponViewModel weaponViewModel = Weapons[i];
                weaponViewModel.IsNext = i == _model.CurrentWeaponIndex;
            }
        }

        private void UpdateFromModel(BuildModel model) {
            _isUpdatingFromModel = true;

            Perks.Clear();
            foreach (Perk perk in model.Perks) {
                Perks.Add(new PerkViewModel(perk));
            }

            Weapons.Clear();
            foreach (Weapon weapon in model.Weapons) {
                Weapons.Add(new WeaponViewModel(weapon));
            }

            ReplaceFireBulletsWith500Points = model.Settings.ReplaceFireBulletsWith500Points;
            ReplacePlasmaOverloadWith500Points = model.Settings.ReplacePlasmaOverloadWith500Points;

            _isUpdatingFromModel = false;
        }
    }
}
