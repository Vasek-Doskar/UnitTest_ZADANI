namespace UnitTest_ZADANI
{
    public class SmartHomeThermostat
    {
        /// <summary>
        /// Aktuálně naměřená teplota v místnosti (°C).
        /// </summary>
        public double CurrentTemperature { get; private set; }

        /// <summary>
        /// Požadovaná (cílová) teplota (°C).
        /// </summary>
        public double TargetTemperature { get; private set; }

        /// <summary>
        /// Udává, zda je aktivní úsporný ECO režim.
        /// </summary>
        public bool IsEcoModeActive { get; private set; }

        /// <summary>
        /// Udává, zda je termostat uzamčen (rodičovský zámek).
        /// </summary>
        public bool IsLocked { get; private set; }

        private readonly List<string> _systemLogs;

        /// <summary> Minimální povolená cílová teplota (5.0 °C). </summary>
        public const double MinTemp = 5.0;

        /// <summary> Maximální povolená cílová teplota (35.0 °C). </summary>
        public const double MaxTemp = 35.0;

        /// <summary>
        /// Inicializuje novou instanci termostatu s výchozími hodnotami.
        /// </summary>
        /// <param name="initialCurrentTemp">Počáteční naměřená teplota (°C).</param>
        /// <param name="initialTargetTemp">Počáteční cílová teplota (°C).</param>
        /// <exception cref="ArgumentOutOfRangeException">Vyhozeno, pokud je naměřená teplota mimo fyzikální rozsah senzoru (-20 °C až 60 °C).</exception>
        public SmartHomeThermostat(double initialCurrentTemp, double initialTargetTemp)
        {
            if (initialCurrentTemp < -20.0 || initialCurrentTemp > 60.0)
                throw new ArgumentOutOfRangeException(nameof(initialCurrentTemp), "Měřená teplota je mimo fyzikální senzorový rozsah.");

            CurrentTemperature = initialCurrentTemp;
            TargetTemperature = ClampTemperature(initialTargetTemp);
            IsEcoModeActive = false;
            IsLocked = false;
            _systemLogs = new List<string>();
        }

        /// <summary>
        /// Ručně nastaví novou cílovou teplotu. Teplota je automaticky ořezána do povoleného rozsahu (5.0 až 35.0 °C).
        /// </summary>
        /// <param name="newTemperature">Požadovaná cílová teplota (°C).</param>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je termostat uzamčen nebo je aktivní ECO režim.</exception>
        public void SetTargetTemperature(double newTemperature)
        {
            EnsureNotLocked();
            if (IsEcoModeActive)
                throw new InvalidOperationException("Nelze měnit teplotu ručně, pokud je aktivní úsporný ECO režim.");

            TargetTemperature = ClampTemperature(newTemperature);
            _systemLogs.Add($"Cílová teplota změněna na {TargetTemperature} °C");
        }

        /// <summary>
        /// Aktualizuje hodnotu aktuální teploty naměřené senzorem.
        /// </summary>
        /// <param name="newTemperature">Nová naměřená teplota ze senzoru (°C).</param>
        /// <exception cref="ArgumentOutOfRangeException">Vyhozeno, pokud je teplota mimo fyzikální rozsah senzoru (-20 °C až 60 °C).</exception>
        public void UpdateCurrentTemperature(double newTemperature)
        {
            if (newTemperature < -20.0 || newTemperature > 60.0)
                throw new ArgumentOutOfRangeException(nameof(newTemperature), "Teplota ze senzoru je mimo rozsah.");

            CurrentTemperature = newTemperature;
        }

        /// <summary>
        /// Zapne úsporný ECO režim a přenastaví cílovou teplotu na zadanou ECO hodnotu.
        /// </summary>
        /// <param name="ecoTemperature">Cílová teplota pro ECO režim v °C (výchozí je 18.0 °C).</param>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je termostat uzamčen.</exception>
        public void EnableEcoMode(double ecoTemperature = 18.0)
        {
            EnsureNotLocked();
            IsEcoModeActive = true;
            TargetTemperature = ClampTemperature(ecoTemperature);
            _systemLogs.Add($"ECO režim zapnut s teplotou {TargetTemperature} °C");
        }

        /// <summary>
        /// Vypne úsporný ECO režim.
        /// </summary>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je termostat uzamčen.</exception>
        public void DisableEcoMode()
        {
            EnsureNotLocked();
            IsEcoModeActive = false;
            _systemLogs.Add("ECO režim vypnut");
        }

        /// <summary>
        /// Zjistí, zda je potřeba spustit vytápění (pokud je aktuální teplota nižší než cílová).
        /// </summary>
        /// <returns><c>true</c>, pokud je aktuální teplota nižší než cílová; jinak <c>false</c>.</returns>
        public bool IsHeatingRequired()
        {
            return CurrentTemperature < TargetTemperature;
        }

        /// <summary>
        /// Vypočítá absolutní odchylku aktuální teploty od cílové teploty.
        /// </summary>
        /// <returns>Kladný rozdíl teplot v °C.</returns>
        public double GetTemperatureDifference()
        {
            return Math.Abs(CurrentTemperature - TargetTemperature);
        }

        /// <summary>
        /// Uzamkne termostat (rodičovský zámek). Zamezí změnám nastavení a režimů.
        /// </summary>
        public void Lock()
        {
            IsLocked = true;
            _systemLogs.Add("Termostat uzamčen");
        }

        /// <summary>
        /// Odemkne termostat a umožní opětovné provádění změn.
        /// </summary>
        public void Unlock()
        {
            IsLocked = false;
            _systemLogs.Add("Termostat odemčen");
        }

        /// <summary>
        /// Ověří, zda se aktuální teplota nachází v zadaném bezpečném rozmezí.
        /// </summary>
        /// <param name="minSafe">Minimální přípustná bezpečná teplota.</param>
        /// <param name="maxSafe">Maximální přípustná bezpečná teplota.</param>
        /// <returns><c>true</c>, pokud je teplota uvnitř rozmezí včetně hraničních hodnot; jinak <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Vyhozeno, pokud je minimální teplota vyšší nebo rovna maximální.</exception>
        public bool IsWithinSafeRange(double minSafe, double maxSafe)
        {
            if (minSafe >= maxSafe)
                throw new ArgumentException("Minimální bezpečná teplota musí být nižší než maximální.");

            return CurrentTemperature >= minSafe && CurrentTemperature <= maxSafe;
        }

        /// <summary>
        /// Vrací ke čtení seznam všech zaznamenaných systémových událostí a změn.
        /// </summary>
        /// <returns>Kolekce systémových zpráv v pořadí, v jakém vznikly.</returns>
        public IReadOnlyList<string> GetSystemLogs()
        {
            return _systemLogs.AsReadOnly();
        }

        private double ClampTemperature(double temp)
        {
            return Math.Max(MinTemp, Math.Min(MaxTemp, temp));
        }

        private void EnsureNotLocked()
        {
            if (IsLocked)
                throw new InvalidOperationException("Termostat je uzamčen. Změnu nelze provést.");
        }
    }
}
