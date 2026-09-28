using System;
using System.Collections.Generic;

namespace UnitTest_ZADANI
{
    /// <summary>
    /// Reprezentuje nákupní košík v e-shopu.
    /// </summary>
    public class EShopCart
    {
        private readonly Dictionary<string, decimal> _items;
        private string _appliedDiscountCode;

        /// <summary> Hranice celkové ceny pro nárok na dopravu zdarma (1000 Kč). </summary>
        public const decimal FreeShippingThreshold = 1000m;

        /// <summary>
        /// Inicializuje nový prázdný nákupní košík.
        /// </summary>
        public EShopCart()
        {
            _items = new Dictionary<string, decimal>();
            _appliedDiscountCode = null;
        }

        // 1. VOID – Přidání zboží do košíku
        /// <summary>
        /// Přidá položku s daným názvem a cenou do košíku.
        /// </summary>
        /// <param name="itemName">Název položky.</param>
        /// <param name="price">Cena položky v Kč.</param>
        /// <exception cref="ArgumentException">Vyhozeno, pokud je název prázdný nebo cena záporná či nulová.</exception>
        public void AddItem(string itemName, decimal price)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException("Název položky nesmí být prázdný.", nameof(itemName));

            if (price <= 0)
                throw new ArgumentException("Cena položky musí být větší než 0.", nameof(price));

            _items[itemName] = price;
        }

        // 2. NÁVRATOVÝ TYP (bool) – Odebrání zboží z košíku
        /// <summary>
        /// Odebere zadanou položku z košíku.
        /// </summary>
        /// <param name="itemName">Název odebírané položky.</param>
        /// <returns><c>true</c>, pokud položka v košíku byla a byla odebrána; jinak <c>false</c>.</returns>
        public bool RemoveItem(string itemName)
        {
            return _items.Remove(itemName);
        }

        // 3. NÁVRATOVÝ TYP (decimal) – Výpočet mecisoučtu cen položek
        /// <summary>
        /// Spočítá celkovou sumu položek v košíku bez započtení slev a dopravy.
        /// </summary>
        /// <returns>Součet cen v Kč.</returns>
        public decimal GetSubtotal()
        {
            decimal total = 0;
            foreach (var item in _items.Values)
            {
                total += item;
            }
            return total;
        }

        // 4. NÁVRATOVÝ TYP (bool) – Uplatnění slevového kódu
        /// <summary>
        /// Uplatní slevový kód na objednávku ("SLEVA10" = 10 % sleva, "SLEVA20" = 20 % sleva).
        /// </summary>
        /// <param name="code">Slevový kód.</param>
        /// <returns><c>true</c>, pokud byl kód platný a uplatněn; jinak <c>false</c>.</returns>
        public bool ApplyDiscountCode(string code)
        {
            if (code == "SLEVA10" || code == "SLEVA20")
            {
                _appliedDiscountCode = code;
                return true;
            }
            return false;
        }

        // 5. VOID – Odebrání slevového kódu
        /// <summary>
        /// Zruší dříve uplatněný slevový kód.
        /// </summary>
        public void RemoveDiscountCode()
        {
            _appliedDiscountCode = null;
        }

        // 6. NÁVRATOVÝ TYP (decimal) – Výpočet ceny dopravy
        /// <summary>
        /// Vypočítá cenu dopravy. Pokud je mezisoučet vyšší nebo roven 1000 Kč, je doprava zdarma (0 Kč), jinak stojí 99 Kč.
        /// </summary>
        /// <returns>Cena dopravy v Kč.</returns>
        public decimal CalculateShippingFee()
        {
            if (_items.Count == 0)
                return 0m;

            return GetSubtotal() >= FreeShippingThreshold ? 0m : 99m;
        }

        // 7. NÁVRATOVÝ TYP (decimal) – Výpočet finální celkové ceny
        /// <summary>
        /// Spočítá konečnou cenu k úhradě vč. uplatněné slevy a poštovného.
        /// </summary>
        /// <returns>Konečná cena v Kč.</returns>
        public decimal GetTotalPrice()
        {
            decimal subtotal = GetSubtotal();
            decimal discountPercentage = 0;

            if (_appliedDiscountCode == "SLEVA10") discountPercentage = 0.10m;
            if (_appliedDiscountCode == "SLEVA20") discountPercentage = 0.20m;

            decimal discountedSubtotal = subtotal * (1 - discountPercentage);
            return discountedSubtotal + CalculateShippingFee();
        }

        // 8. VOID – Vyprázdnění košíku
        /// <summary>
        /// Vymaže všechny položky z košíku a odebere slevový kód.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _appliedDiscountCode = null;
        }

        // 9. NÁVRATOVÝ TYP (int) – Získání počtu položek v košíku
        /// <summary>
        /// Vrací celkový počet unikátních položek v košíku.
        /// </summary>
        /// <returns>Počet položek.</returns>
        public int GetItemCount()
        {
            return _items.Count;
        }

        // 10. NÁVRATOVÝ TYP (bool) – Kontrola možnosti dokončení objednávky
        /// <summary>
        /// Ověří, zda lze košík odeslat k objednávce (musí obsahovat alespoň 1 položku a mezisoučet musí být min. 100 Kč).
        /// </summary>
        /// <returns><c>true</c>, pokud je košík připraven k objednání; jinak <c>false</c>.</returns>
        public bool IsReadyForCheckout()
        {
            return _items.Count > 0 && GetSubtotal() >= 100m;
        }
    }

}