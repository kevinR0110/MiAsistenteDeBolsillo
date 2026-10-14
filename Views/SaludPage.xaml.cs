using System.Globalization;

namespace MiAsistenteDeBolsillo.Views;

public partial class SaludPage : ContentPage
{
    // ===== Constantes de validación (evitan resultados absurdos) =====
    private const double PesoMaximoKg = 500;
    private const double AlturaMaximaM = 2.5;

    public SaludPage()
    {
        InitializeComponent();
    }

    // Evento del botón Calcular: valida, calcula el IMC y muestra la tarjeta
    private async void OnCalcularClicked(object sender, EventArgs e)
    {
        brdResultado.IsVisible = false; // oculta el resultado anterior

        // 1) Campos vacíos
        if (string.IsNullOrWhiteSpace(txtPeso.Text) || string.IsNullOrWhiteSpace(txtAltura.Text))
        {
            await DisplayAlert("Campos vacíos", "Escribe tu peso y tu altura.", "OK");
            return;
        }

        // 2) Que sean números (rechaza letras)
        if (!TryLeerNumero(txtPeso.Text, out double peso) ||
            !TryLeerNumero(txtAltura.Text, out double altura))
        {
            await DisplayAlert("Dato inválido", "Escribe solo números en peso y altura.", "OK");
            return;
        }

        // 3) Cero o negativos
        if (peso <= 0 || altura <= 0)
        {
            await DisplayAlert("Dato inválido", "El peso y la altura deben ser mayores que 0.", "OK");
            return;
        }

        // 4) Valores fuera de lo razonable (ej. altura escrita en cm)
        if (peso > PesoMaximoKg || altura > AlturaMaximaM)
        {
            await DisplayAlert("Dato fuera de rango",
                $"El peso debe ser menor a {PesoMaximoKg} kg y la altura menor a {AlturaMaximaM} m (escríbela en metros).",
                "OK");
            return;
        }

        // Fórmula del IMC: peso / altura²
        double imc = peso / (altura * altura);

        // Diagnóstico y color de la tarjeta
        string diagnostico;
        Color color;

        if (imc < 18.5)
        {
            diagnostico = "Bajo peso";
            color = Color.FromArgb("#2563EB"); // azul
        }
        else if (imc < 25)
        {
            diagnostico = "Normal";
            color = Color.FromArgb("#16A34A"); // verde
        }
        else if (imc < 30)
        {
            diagnostico = "Sobrepeso";
            color = Color.FromArgb("#EA580C"); // naranja
        }
        else
        {
            diagnostico = "Obesidad";
            color = Color.FromArgb("#DC2626"); // rojo
        }

        lblImc.Text = imc.ToString("F1", CultureInfo.InvariantCulture);
        lblEstado.Text = diagnostico;
        brdResultado.BackgroundColor = color;
        brdResultado.IsVisible = true;
    }

    // Mejora B: borra los campos y oculta el resultado
    private void OnLimpiarClicked(object sender, EventArgs e)
    {
        txtPeso.Text = string.Empty;
        txtAltura.Text = string.Empty;
        brdResultado.IsVisible = false;
    }

    // Convierte texto a número sin importar si el usuario usa coma o punto
    private static bool TryLeerNumero(string texto, out double valor)
    {
        texto = texto.Trim().Replace(',', '.');
        return double.TryParse(texto,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out valor);
    }
}