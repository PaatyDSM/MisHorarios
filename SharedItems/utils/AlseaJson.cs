using System;
using System.Collections.ObjectModel;
using Windows.Data.Json;

namespace PaatyDSM.Json
{
    internal class AlseaJson
    {
        private const string asignacionesKey = "asignaciones";
        private const string fechaConsultaKey = "fechaConsulta";
        private const string legajoKey = "legajo";

        public AlseaJson()
        {
            Asignaciones = new ObservableCollection<Dias>();
            FechaConsulta = "";
            Legajo = "";
        }

        public AlseaJson(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            FechaConsulta = jsonObject.GetNamedString(fechaConsultaKey, "");
            Legajo = jsonObject.GetNamedString(legajoKey, "");
            foreach (IJsonValue jsonValue in jsonObject.GetNamedArray(asignacionesKey, new JsonArray()))
            {
                if (jsonValue.ValueType == JsonValueType.Object)
                {
                    Asignaciones.Add(new Dias(jsonValue.GetObject()));
                }
            }
        }

        public string FechaConsulta { get; set; }

        public string Legajo { get; set; }

        public ObservableCollection<Dias> Asignaciones { get; }
    }

    internal class Dias
    {
        private const string fechaKey = "fecha";
        private const string horaEntradaKey = "horaEntrada";
        private const string horaSalidaKey = "horaSalida";
        private const string tiendaKey = "tienda";

        private string fecha;
        private string horaEntrada;
        private string horaSalida;
        private string tienda;

        public Dias()
        {
            Fecha = "";
            HoraEntrada = "";
            HoraSalida = "";
            Tienda = "";
        }

        public Dias(JsonObject jsonObject)
        {
            Fecha = jsonObject.GetNamedString(fechaKey, "");
            HoraEntrada = jsonObject.GetNamedString(horaEntradaKey, "");
            HoraSalida = jsonObject.GetNamedString(horaSalidaKey, "");
            Tienda = jsonObject.GetNamedString(tiendaKey, "");
        }

        public JsonObject ToJsonObject()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject.SetNamedValue(fechaKey, JsonValue.CreateStringValue(Fecha));
            jsonObject.SetNamedValue(horaEntradaKey, JsonValue.CreateStringValue(HoraEntrada));
            jsonObject.SetNamedValue(horaSalidaKey, JsonValue.CreateStringValue(HoraSalida));
            jsonObject.SetNamedValue(tiendaKey, JsonValue.CreateStringValue(Tienda));

            return jsonObject;
        }

        public string Fecha
        {
            get
            {
                return fecha;
            }
            set
            {
                fecha = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public string HoraEntrada
        {
            get
            {
                return horaEntrada;
            }
            set
            {
                horaEntrada = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public string HoraSalida
        {
            get
            {
                return horaSalida;
            }
            set
            {
                horaSalida = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public string Tienda
        {
            get
            {
                return tienda;
            }
            set
            {
                tienda = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
    }
}
