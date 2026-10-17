using System.Globalization;

namespace MiAsistenteDeBolsillo.Views;

public partial class DineroPage : ContentPage
{
    // ===== Tasas de cambio (constantes). Ajústalas a la tasa del día =====
    private const double HnlPorUsd = 24.75; // 1 USD = 24.75 HNL
    private const double UsdPorEur = 1.08;  // 1 EUR = 1.08 USD
    private const double MontoMaximo = 1_000_000_000;

    // Lista de monedas que aparece en ambos Picker
    // Siglas de las monedas: se usan en los cálculos
    private static readonly string[] Monedas = { "HNL", "USD", "EUR" };

    // Texto que ve el usuario en la lista desplegable (mismo orden que Monedas)
    private static readonly string[] NombresMonedas =
    {
        "L  ·  HNL - Lempira hondureño",
        "$  ·  USD - Dólar estadounidense",
        "€  ·  EUR - Euro"
    };

    public DineroPage()
    {
        InitializeComponent();
        pckOrigen.ItemsSource = NombresMonedas;
        pckDestino.ItemsSource = NombresMonedas;
    }

    // Evento del botón Convertir
    private async void OnConvertirClicked(object sender, EventArgs e)
    {
        brdResultado.IsVisible = false;

        // 1) Campo vacío
        if (string.IsNullOrWhiteSpace(txtMonto.Text))
        {
            await DisplayAlert("Campo vacío", "Escribe el monto a convertir.", "OK");
            return;
        }

        // 2) Que sea número (rechaza letras)
        if (!TryLeerNumero(txtMonto.Text, out double monto))
        {
            await DisplayAlert("Dato inválido", "Escribe solo números en el monto.", "OK");
            return;
        }

        // 3) Cero o negativo
        if (monto <= 0)
        {
            await DisplayAlert("Dato inválido", "El monto debe ser mayor que 0.", "OK");
            return;
        }

        // 4) Monto absurdamente grande
        if (monto > MontoMaximo)
        {
            await DisplayAlert("Dato fuera de rango", "El monto es demasiado grande.", "OK");
            return;
        }

        // 5) Moneda sin elegir (SelectedIndex = -1 significa "nada seleccionado")
        if (pckOrigen.SelectedIndex == -1 || pckDestino.SelectedIndex == -1)
        {
            await DisplayAlert("Falta una moneda", "Elige la moneda de origen y la de destino.", "OK");
            return;
        }

        string origen = Monedas[pckOrigen.SelectedIndex];
        string destino = Monedas[pckDestino.SelectedIndex];

        // Siempre pasamos por dólares: origen -> USD -> destino
        double enUsd = AUsd(origen, monto);
        double resultado = DesdeUsd(destino, enUsd);

        lblDetalle.Text = $"{Formatear(origen, monto)} equivale a";
        lblResultado.Text = Formatear(destino, resultado); // mejora F: símbolo
        brdResultado.IsVisible = true;
    }

    // Mejora C: cambia origen por destino usando SelectedIndex
    private void OnIntercambiarClicked(object sender, EventArgs e)
    {
        int temporal = pckOrigen.SelectedIndex;
        pckOrigen.SelectedIndex = pckDestino.SelectedIndex;
        pckDestino.SelectedIndex = temporal;
        brdResultado.IsVisible = false; // el resultado anterior ya no aplica
    }

    // Mejora B: borra los campos y oculta el resultado
    private void OnLimpiarClicked(object sender, EventArgs e)
    {
        txtMonto.Text = string.Empty;
        pckOrigen.SelectedIndex = -1;
        pckDestino.SelectedIndex = -1;
        brdResultado.IsVisible = false;
    }

    // ===== Métodos auxiliares =====

    // Convierte de cualquier moneda a dólares
    private static double AUsd(string moneda, double monto) => moneda switch
    {
        "HNL" => monto / HnlPorUsd,
        "EUR" => monto * UsdPorEur,
        _ => monto // USD
    };

    // Convierte de dólares a cualquier moneda
    private static double DesdeUsd(string moneda, double usd) => moneda switch
    {
        "HNL" => usd * HnlPorUsd,
        "EUR" => usd / UsdPorEur,
        _ => usd // USD
    };

    // Símbolo de cada moneda (mejora F)
    private static string Simbolo(string moneda) => moneda switch
    {
        "HNL" => "L",
        "USD" => "$",
        "EUR" => "€",
        _ => ""
    };

    // Ejemplo: L 2,475.00 / $ 100.00 / € 92.59
    private static string Formatear(string moneda, double valor) =>
        $"{Simbolo(moneda)} {valor.ToString("N2", CultureInfo.InvariantCulture)}";

    private static bool TryLeerNumero(string texto, out double valor)
    {
        texto = texto.Trim().Replace(',', '.');
        return double.TryParse(texto,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out valor);
    }
}