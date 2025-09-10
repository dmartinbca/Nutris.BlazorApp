using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace NutrisBlazor.Services
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        Dictionary<string, string> Translations { get; }
        event Action OnLanguageChanged;
        Task InitializeAsync();
        Task ChangeLanguageAsync(string language);
        string Translate(string key);
        string this[string key] { get; }
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private Dictionary<string, string> _translations = new Dictionary<string, string>();
        private string _currentLanguage = "en";

        public string CurrentLanguage => _currentLanguage ?? "en";
        public Dictionary<string, string> Translations => _translations;
        public event Action OnLanguageChanged;

        public LocalizationService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("LocalizationService: Starting initialization");

                // Intentar obtener el idioma guardado
                try
                {
                    var savedLanguage = await _localStorage.GetItemAsync<string>("language");
                    if (!string.IsNullOrEmpty(savedLanguage) && savedLanguage != "language")
                    {
                        _currentLanguage = savedLanguage;
                        Console.WriteLine($"LocalizationService: Found saved language: {savedLanguage}");
                    }
                    else
                    {
                        _currentLanguage = "en";
                        await _localStorage.SetItemAsync("language", _currentLanguage);
                        Console.WriteLine($"LocalizationService: No saved language, using default: {_currentLanguage}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"LocalizationService: Error reading saved language: {ex.Message}");
                    _currentLanguage = "en";
                }

                await LoadTranslationsAsync(_currentLanguage);
                Console.WriteLine($"LocalizationService: Initialization complete with {_translations.Count} translations");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LocalizationService: Error initializing - {ex.Message}");
                LoadDefaultTranslations();
            }
        }

        public async Task ChangeLanguageAsync(string language)
        {
            try
            {
                Console.WriteLine($"ChangeLanguageAsync: Changing from {_currentLanguage} to {language}");

                if (_currentLanguage == language)
                {
                    Console.WriteLine($"ChangeLanguageAsync: Language is already {language}, skipping");
                    return;
                }

                _currentLanguage = language;
                await _localStorage.SetItemAsync("language", language);
                await LoadTranslationsAsync(language);

                Console.WriteLine($"ChangeLanguageAsync: Language changed to {language}, invoking OnLanguageChanged event");
                OnLanguageChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChangeLanguageAsync: Error - {ex.Message}");
            }
        }

        private async Task LoadTranslationsAsync(string language)
        {
            try
            {
                Console.WriteLine($"LoadTranslationsAsync: Loading {language}.json");
                var response = await _httpClient.GetAsync($"locales/{language}.json");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"LoadTranslationsAsync: JSON received, processing...");

                    // Parse as JsonDocument to handle complex structures
                    using var document = JsonDocument.Parse(json);

                    _translations.Clear();

                    // Process the root element
                    ProcessJsonElement(document.RootElement, "");

                    Console.WriteLine($"LoadTranslationsAsync: Loaded {_translations.Count} translations for {language}");

                    // Log sample translations for verification
                    if (_translations.ContainsKey("message.Username"))
                        Console.WriteLine($"Sample: message.Username = {_translations["message.Username"]}");
                    if (_translations.ContainsKey("message.Password"))
                        Console.WriteLine($"Sample: message.Password = {_translations["message.Password"]}");
                }
                else
                {
                    Console.WriteLine($"LoadTranslationsAsync: Failed to load {language}.json - Status: {response.StatusCode}");
                    LoadDefaultTranslations();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadTranslationsAsync: Exception - {ex.Message}");
                LoadDefaultTranslations();
            }
        }

        private void ProcessJsonElement(JsonElement element, string prefix)
        {
            try
            {
                switch (element.ValueKind)
                {
                    case JsonValueKind.Object:
                        foreach (var property in element.EnumerateObject())
                        {
                            var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                            ProcessJsonElement(property.Value, key);
                        }
                        break;

                    case JsonValueKind.Array:
                        // For arrays, store as comma-separated string or individually
                        var arrayIndex = 0;
                        var arrayValues = new List<string>();
                        foreach (var item in element.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                            {
                                var value = item.GetString();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    arrayValues.Add(value);
                                    // Also store individual array items with index
                                    _translations[$"{prefix}[{arrayIndex}]"] = value;
                                }
                            }
                            else if (item.ValueKind == JsonValueKind.Object)
                            {
                                ProcessJsonElement(item, $"{prefix}[{arrayIndex}]");
                            }
                            arrayIndex++;
                        }
                        // Store array as comma-separated string
                        if (arrayValues.Count > 0)
                        {
                            _translations[prefix] = string.Join(", ", arrayValues);
                        }
                        break;

                    case JsonValueKind.String:
                        var stringValue = element.GetString();
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            _translations[prefix] = stringValue;
                        }
                        break;

                    case JsonValueKind.Number:
                        _translations[prefix] = element.GetRawText();
                        break;

                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        _translations[prefix] = element.GetBoolean().ToString();
                        break;

                    case JsonValueKind.Null:
                        _translations[prefix] = "";
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProcessJsonElement: Error processing {prefix} - {ex.Message}");
            }
        }

        private void LoadDefaultTranslations()
        {
            Console.WriteLine("LoadDefaultTranslations: Loading default English translations");
            _translations = new Dictionary<string, string>
            {
                ["message.Username"] = "Username",
                ["message.Password"] = "Password",
                ["message.Your account name or password is incorrect"] = "Your account name or password is incorrect",
                ["message.Incorrect credentials Please verify your username and password"] = "Incorrect credentials. Please verify your username and password",
                ["message.Rememberme"] = "Remember me",
                ["message.Forgot your password"] = "Forgot your password?",
                ["message.Log in"] = "Log in",
                ["message.Loading"] = "Loading...",
                ["message.Welcome"] = "Welcome",
                ["NavBar.Home"] = "Home",
                ["NavBar.YourProducts"] = "Your Products",
                ["NavBar.Customize"] = "Customize",
                ["NavBar.YourOrder"] = "Your Order"
            };
        }

        public string Translate(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            // Try exact match first
            if (_translations.TryGetValue(key, out var translation))
                return translation;

            // Try with spaces trimmed
            var trimmedKey = key.Trim();
            if (_translations.TryGetValue(trimmedKey, out translation))
                return translation;

            // Try replacing spaces with different variations
            var keyWithSpace = key.Replace(" ", " "); // Sometimes there are different space characters
            if (_translations.TryGetValue(keyWithSpace, out translation))
                return translation;

            // Return the key if no translation found
            return key;
        }

        public string this[string key] => Translate(key);
    }
}