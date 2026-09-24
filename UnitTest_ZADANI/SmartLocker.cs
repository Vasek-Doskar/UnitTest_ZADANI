using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest_ZADANI
{
    public class SmartLocker
    {
        private string _pinCode;
        private int _failedAttempts;

        /// <summary> Označuje, zda je schránka fyzicky otevřená. </summary>
        public bool IsOpen { get; private set; }

        /// <summary> Označuje, zda je schránka zablokovaná z důvodu překročení chybného PINu. </summary>
        public bool IsBlocked { get; private set; }

        /// <summary> Název uloženého předmětu/zásilky (null, pokud je schránka prázdná). </summary>
        public string StoredItem { get; private set; }

        /// <summary> Maximální počet povolených neúspěšných pokusů o zadání PINu před zablokováním (3). </summary>
        public const int MaxFailedAttempts = 3;

        private readonly List<string> _accessLogs;

        /// <summary>
        /// Inicializuje novou instanci chytré schránky. Výchozí stav je zavřená a prázdná.
        /// </summary>
        /// <param name="initialPin">4místný číselný PIN pro přístup.</param>
        /// <exception cref="ArgumentException">Vyhozeno, pokud PIN není přesně 4místné číslo.</exception>
        public SmartLocker(string initialPin)
        {
            if (!IsValidPinFormat(initialPin))
                throw new ArgumentException("PIN musí obsahovat přesně 4 číslice.", nameof(initialPin));

            _pinCode = initialPin;
            IsOpen = false;
            IsBlocked = false;
            StoredItem = null;
            _failedAttempts = 0;
            _accessLogs = new List<string>();
        }

        // 1. NÁVRATOVÝ TYP (bool) – Pokus o otevření schránky pomocí PINu
        /// <summary>
        /// Pokusí se otevřít schránku po zadání PIN kódu.
        /// </summary>
        /// <param name="enteredPin">Zadaný PIN kód.</param>
        /// <returns><c>true</c>, pokud byl PIN správný a schránka se otevřela; jinak <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je schránka zablokovaná.</exception>
        public bool UnlockWithPin(string enteredPin)
        {
            if (IsBlocked)
                throw new InvalidOperationException("Schránka je zablokovaná. Nelze použít PIN.");

            if (enteredPin == _pinCode)
            {
                IsOpen = true;
                _failedAttempts = 0;
                _accessLogs.Add("Úspěšné odemčení PINem.");
                return true;
            }

            _failedAttempts++;
            _accessLogs.Add($"Neúspěšný pokus o odemčení PINem ({_failedAttempts}/{MaxFailedAttempts}).");

            if (_failedAttempts >= MaxFailedAttempts)
            {
                IsBlocked = true;
                _accessLogs.Add("Schránka byla zablokována kvůli překročení pokusů.");
            }

            return false;
        }

        // 2. VOID – Zavření / zaklapnutí schránky
        /// <summary>
        /// Zavře otevřenou schránku.
        /// </summary>
        public void Close()
        {
            IsOpen = false;
            _accessLogs.Add("Schránka zavřena.");
        }

        // 3. VOID – Vložení předmětu do schránky
        /// <summary>
        /// Vloží předmět do otevřené a prázdné schránky.
        /// </summary>
        /// <param name="itemName">Název vkládaného předmětu.</param>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je schránka zavřená nebo již obsahuje jiný předmět.</exception>
        /// <exception cref="ArgumentException">Vyhozeno, pokud je název předmětu prázdný.</exception>
        public void DepositItem(string itemName)
        {
            if (!IsOpen)
                throw new InvalidOperationException("Do zavřené schránky nelze vložit předmět.");

            if (StoredItem != null)
                throw new InvalidOperationException("Schránka již obsahuje předmět.");

            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException("Název předmětu nesmí být prázdný.", nameof(itemName));

            StoredItem = itemName;
            _accessLogs.Add($"Vložen předmět: {itemName}");
        }

        // 4. NÁVRATOVÝ TYP (string) – Vynešení / odebrání předmětu ze schránky
        /// <summary>
        /// Vyjme uložený předmět z otevřené schránky.
        /// </summary>
        /// <returns>Název vyzvednutého předmětu.</returns>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je schránka zavřená nebo v ní nic není.</exception>
        public string RetrieveItem()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Ze zavřené schránky nelze vyzvednout předmět.");

            if (StoredItem == null)
                throw new InvalidOperationException("Schránka je prázdná.");

            string item = StoredItem;
            StoredItem = null;
            _accessLogs.Add($"Vyzvednut předmět: {item}");
            return item;
        }

        // 5. NÁVRATOVÝ TYP (bool) – Změna PIN kódu
        /// <summary>
        /// Změní výchozí PIN kód na nový po ověření starého PINu.
        /// </summary>
        /// <param name="oldPin">Stávající PIN kód.</param>
        /// <param name="newPin">Nový 4místný PIN kód.</param>
        /// <returns><c>true</c>, pokud změna proběhla úspěšně; jinak <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je schránka zablokovaná.</exception>
        /// <exception cref="ArgumentException">Vyhozeno, pokud nový PIN nemá platný formát.</exception>
        public bool ChangePin(string oldPin, string newPin)
        {
            if (IsBlocked)
                throw new InvalidOperationException("Schránka je zablokovaná. Změnu PINu nelze provést.");

            if (!IsValidPinFormat(newPin))
                throw new ArgumentException("Nový PIN musí být přesně 4 čísla.", nameof(newPin));

            if (oldPin != _pinCode)
            {
                _accessLogs.Add("Selhala změna PINu: Neplatný starý PIN.");
                return false;
            }

            _pinCode = newPin;
            _accessLogs.Add("PIN úspěšně změněn.");
            return true;
        }

        // 6. VOID – Odblokování schránky klíčem správce / Master reset
        /// <summary>
        /// Odblokuje zablokovanou schránku a vynuluje čítač neúspěšných pokusů.
        /// </summary>
        /// <param name="masterKey">Master klíč správce (musí být "ADMIN1234").</param>
        /// <exception cref="ArgumentException">Vyhozeno, pokud je zadaný nesprávný Master klíč.</exception>
        public void MasterUnblock(string masterKey)
        {
            if (masterKey != "ADMIN1234")
                throw new ArgumentException("Neplatný Master klíč pro odblokování.", nameof(masterKey));

            IsBlocked = false;
            _failedAttempts = 0;
            _accessLogs.Add("Schránka odblokována správcem.");
        }

        // 7. NÁVRATOVÝ TYP (bool) – Kontrola, zda je schránka prázdná
        /// <summary>
        /// Zjistí, zda je schránka v současné chvíli prázdná.
        /// </summary>
        /// <returns><c>true</c>, pokud schránka neobsahuje žádný předmět; jinak <c>false</c>.</returns>
        public bool IsEmpty()
        {
            return StoredItem == null;
        }

        // 8. NÁVRATOVÝ TYP (int) – Získání počtu zbývajících pokusů před zablokováním
        /// <summary>
        /// Vrací počet zbývajících pokusů pro zadání PINu, než se schránka zablokuje.
        /// </summary>
        /// <returns>Počet pokusů v rozmezí 0 až 3.</returns>
        public int GetRemainingAttempts()
        {
            return Math.Max(0, MaxFailedAttempts - _failedAttempts);
        }

        // 9. VOID – Nouzové vyprázdnění schránky
        /// <summary>
        /// Provede okamžité vyprázdnění a zavření schránky při nouzovém stavu.
        /// </summary>
        public void EmergencyReset()
        {
            StoredItem = null;
            IsOpen = false;
            _accessLogs.Add("Nouzový reset schránky spuštěn.");
        }

        // 10. NÁVRATOVÝ TYP (IReadOnlyList<string>) – Získání přístupových logů
        /// <summary>
        /// Vrací historii všech akcí a přístupů ke schránce.
        /// </summary>
        /// <returns>Kolekce zpráv protokolu o přístupech.</returns>
        public IReadOnlyList<string> GetAccessLogs()
        {
            return _accessLogs.AsReadOnly();
        }

        private bool IsValidPinFormat(string pin)
        {
            return !string.IsNullOrEmpty(pin) && pin.Length == 4 && int.TryParse(pin, out _);
        }
    }
}
