using System;
using System.Collections.ObjectModel;

using Windows.Data.Json;

namespace JSonDbUtilities
{
    internal class User
    {
        private const string legajokey = "legajo";
        private const string fechaConsultaKey = "fechaConsulta";
        private const string asignacionesKey = "asignaciones";

        private string legajo;
        private string fechaConsulta;
        private ObservableCollection<WorkDays> asignaciones;

        public User()
        {
            legajo = "";
            fechaConsulta = "";
            asignaciones = new ObservableCollection<WorkDays>();
        }

        public User(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            Legajo = jsonObject.GetNamedString(legajokey, "");
            FechaConsulta = jsonObject.GetNamedString(fechaConsultaKey, "");
            Asignaciones = new ObservableCollection<WorkDays>();

            foreach (IJsonValue jsonValue in jsonObject.GetNamedArray(asignacionesKey, new JsonArray()))
            {
                if (jsonValue.ValueType == JsonValueType.Object)
                {
                    asignaciones.Add(new WorkDays(jsonValue.GetObject()));
                }
            }
        }

        public string Stringify()
        {
            JsonArray jsonArray = new JsonArray();
            foreach (WorkDays fecha in Asignaciones)
            {
                jsonArray.Add(fecha.ToJsonObject());
            }

            JsonObject jsonObject = new JsonObject
            {
                [legajokey] = JsonValue.CreateStringValue(Legajo),
                [fechaConsultaKey] = JsonValue.CreateStringValue(FechaConsulta),
                [asignacionesKey] = jsonArray
            };

            return jsonObject.Stringify();
        }

        public string Legajo
        {
            get
            {
                return legajo;
            }
            set
            {
                legajo = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public string FechaConsulta
        {
            get
            {
                return fechaConsulta;
            }
            set
            {
                fechaConsulta = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public ObservableCollection<WorkDays> Asignaciones { get; }
    }

    internal class WorkDays
    {
        private const string fechaKey = "fecha";
        private const string horaEntradaKey = "horaEntrada";
        private const string horaSalidaKey = "horaSalida";
        private const string tiendaKey = "tienda";
        private const string asignacionesKey = "asignaciones";

        private string fecha;
        private string horaEntrada;
        private string horaSalida;
        private string tienda;

        public WorkDays()
        {
            fecha = "";
            horaEntrada = "";
            horaSalida = "";
            tienda = "";
        }

        public WorkDays(JsonObject jsonObject)
        {
            JsonObject WorkDaysObject = jsonObject.GetNamedObject(asignacionesKey, null);
            if (WorkDaysObject != null)
            {
                Fecha = WorkDaysObject.GetNamedString(fechaKey, "");
                HoraEntrada = WorkDaysObject.GetNamedString(horaEntradaKey, "");
                HoraSalida = WorkDaysObject.GetNamedString(horaSalidaKey, "");
                Tienda = WorkDaysObject.GetNamedString(tiendaKey, "");
            }
        }

        public JsonObject ToJsonObject()
        {
            JsonObject schoolObject = new JsonObject();
            schoolObject.SetNamedValue(fechaKey, JsonValue.CreateStringValue(Fecha));
            schoolObject.SetNamedValue(horaEntradaKey, JsonValue.CreateStringValue(HoraEntrada));
            schoolObject.SetNamedValue(horaSalidaKey, JsonValue.CreateStringValue(HoraSalida));
            schoolObject.SetNamedValue(tiendaKey, JsonValue.CreateStringValue(Tienda));

            return schoolObject;
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