using System;
using System.Collections.Generic;

namespace UnitTest_ZADANI
{
    /// <summary>
    /// Reprezentuje automatický kávovar se správou surovin a údržby.
    /// </summary>
    public class CoffeeMachine
    {
        /// <summary> Aktuální množství vody v nádržce (ml). Max 1500 ml. </summary>
        public int WaterLevelMl { get; private set; }

        /// <summary> Aktuální množství kávových zrn (g). Max 250 g. </summary>
        public int BeansLevelGrams { get; private set; }

        /// <summary> Počet připravených káv od posledního vyprázdnění odpadní nádoby na sedlinu. </summary>
        public int WasteContainerCount { get; private set; }

        /// <summary> Označuje, zda je kávovar zapnutý. </summary>
        public bool IsPoweredOn { get; private set; }

        /// <summary> Maximální kapacita odpadní nádoby na sedlinu (počet káv). </summary>
        public const int MaxWasteCapacity = 5;

        private readonly List<string> _statusLogs;

        /// <summary>
        /// Inicializuje nový kávovar. Výchozí stav je vypnutý a bez surovin.
        /// </summary>
        public CoffeeMachine()
        {
            WaterLevelMl = 0;
            BeansLevelGrams = 0;
            WasteContainerCount = 0;
            IsPoweredOn = false;
            _statusLogs = new List<string>();
        }

        // 1. VOID – Zapnutí kávovaru
        /// <summary>
        /// Zapne kávovar a provede úvodní proplach.
        /// </summary>
        public void PowerOn()
        {
            IsPoweredOn = true;
            _statusLogs.Add("Kávovar zapnut.");
        }

        // 2. VOID – Vypnutí kávovaru
        /// <summary>
        /// Vypne kávovar.
        /// </summary>
        public void PowerOff()
        {
            IsPoweredOn = false;
            _statusLogs.Add("Kávovar vypnut.");
        }

        // 3. VOID – Doplňování vody
        /// <summary>
        /// Doplní vodu do nádržky kávovaru. Maximální kapacita je 1500 ml.
        /// </summary>
        /// <param name="amountMl">Množství přidávané vody v ml.</param>
        /// <exception cref="ArgumentException">Vyhozeno, pokud je množství vody 0 nebo záporné.</exception>
        public void RefillWater(int amountMl)
        {
            if (amountMl <= 0)
                throw new ArgumentException("Množství doplňované vody musí být větší než 0.", nameof(amountMl));

            WaterLevelMl = Math.Min(1500, WaterLevelMl + amountMl);
            _statusLogs.Add($"Doplněna voda: +{amountMl} ml");
        }

        // 4. VOID – Doplňování kávových zrn
        /// <summary>
        /// Doplní kávová zrna. Maximální kapacita je 250 g.
        /// </summary>
        /// <param name="amountGrams">Množství přidávané kávy v gramech.</param>
        /// <exception cref="ArgumentException">Vyhozeno, pokud je množství zrn 0 nebo záporné.</exception>
        public void RefillBeans(int amountGrams)
        {
            if (amountGrams <= 0)
                throw new ArgumentException("Množství doplňované kávy musí být větší než 0.", nameof(amountGrams));

            BeansLevelGrams = Math.Min(250, BeansLevelGrams + amountGrams);
            _statusLogs.Add($"Doplněna káva: +{amountGrams} g");
        }

        // 5. NÁVRATOVÝ TYP (bool) – Příprava nápoje
        /// <summary>
        /// Připraví zadaný typ kávy, pokud je přístroj zapnutý a je dostatek surovin i místa v odpadu.
        /// </summary>
        /// <param name="waterNeededMl">Potřebná voda v ml.</param>
        /// <param name="beansNeededGrams">Potřebná káva v g.</param>
        /// <returns><c>true</c>, pokud se káva úspěšně připravila; jinak <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je kávovar vypnutý.</exception>
        public bool MakeCoffee(int waterNeededMl, int beansNeededGrams)
        {
            if (!IsPoweredOn)
                throw new InvalidOperationException("Kávovar je vypnutý. Nelze připravit kávu.");

            if (WaterLevelMl < waterNeededMl || BeansLevelGrams < beansNeededGrams || WasteContainerCount >= MaxWasteCapacity)
            {
                _statusLogs.Add("Příprava kávy selhala: Nedostatek surovin nebo plný odpad.");
                return false;
            }

            WaterLevelMl -= waterNeededMl;
            BeansLevelGrams -= beansNeededGrams;
            WasteContainerCount++;
            _statusLogs.Add("Káva byla úspěšně připravena.");
            return true;
        }

        // 6. VOID – Vyprázdnění odpadní nádoby
        /// <summary>
        /// Vyprázdní nádobu na kávovou sedlinu.
        /// </summary>
        public void EmptyWasteContainer()
        {
            WasteContainerCount = 0;
            _statusLogs.Add("Odpadní nádoba vyprázdněna.");
        }

        // 7. NÁVRATOVÝ TYP (bool) – Kontrola, zda kávovar vyžaduje údržbu
        /// <summary>
        /// Zjistí, zda kávovar vyžaduje pozornost (doplnění vody, zrn nebo vysypání sedliny).
        /// </summary>
        /// <returns><c>true</c>, pokud je potřeba zásah uživatele; jinak <c>false</c>.</returns>
        public bool NeedsMaintenance()
        {
            return WaterLevelMl < 50 || BeansLevelGrams < 10 || WasteContainerCount >= MaxWasteCapacity;
        }

        // 8. NÁVRATOVÝ TYP (int) – Odhad zbývajícího počtu káv
        /// <summary>
        /// Spočítá, kolik standardních porcí kávy (100 ml vody, 10 g zrn) lze ještě připravit.
        /// </summary>
        /// <returns>Počet káv, které je možné připravit ze stávajících zásob.</returns>
        public int EstimateRemainingCoffees()
        {
            int coffeesFromWater = WaterLevelMl / 100;
            int coffeesFromBeans = BeansLevelGrams / 10;
            return Math.Min(coffeesFromWater, coffeesFromBeans);
        }

        // 9. VOID – Rychlé propláchnutí
        /// <summary>
        /// Provede propláchnutí kávovaru a spotřebuje 30 ml vody.
        /// </summary>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud kávovar běží nebo nemá dostatek vody (min 30 ml).</exception>
        public void FlushSystem()
        {
            if (!IsPoweredOn)
                throw new InvalidOperationException("Nelze propláchnout vypnutý kávovar.");

            if (WaterLevelMl < 30)
                throw new InvalidOperationException("Nedostatek vody pro propláchnutí.");

            WaterLevelMl -= 30;
            _statusLogs.Add("Systém propláchnut.");
        }

        // 10. NÁVRATOVÝ TYP (IReadOnlyList<string>) – Získání logů
        /// <summary>
        /// Vrací historii stavových zpráv kávovaru.
        /// </summary>
        /// <returns>Seznam systémových hlášení.</returns>
        public IReadOnlyList<string> GetStatusLogs()
        {
            return _statusLogs.AsReadOnly();
        }
    }
}