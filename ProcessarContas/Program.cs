using ProcessarContas.Logica;
using System.Configuration;
using System.Reflection;


if (!int.TryParse(ConfigurationManager.AppSettings["TempoEsperaSegundos"], out var tempoEsperaSegundos))
	tempoEsperaSegundos = 30;

if (!int.TryParse(ConfigurationManager.AppSettings["NrProcessosParalelos"], out var nrProcessosParalelos))
	nrProcessosParalelos = 4;

var cont = 1;

object sync = new object();

while (cont <= nrProcessosParalelos)
{
	lock (sync)
	{
		Thread t = new Thread(ProcessadorContas);
		t.Start();
		Thread.Sleep(TimeSpan.FromSeconds(1));
	}

	if (cont == nrProcessosParalelos)
	{
		Thread.Sleep(TimeSpan.FromSeconds(tempoEsperaSegundos));
		cont = 0;
	}

	cont++;
}

Console.ReadKey();

void ProcessadorContas(object? obj)
{
	try
	{
		var caminhoArquivos = ConfigurationManager.AppSettings["CaminhoArquivos"];

		if (!Directory.Exists(caminhoArquivos))
			throw new Exception("Caminho dos arquivos não localizado, verifique o arquivo App.config");

		if (!int.TryParse(ConfigurationManager.AppSettings["NrRegistrosProcessar"], out var nrRegistrosProcessar))
			nrRegistrosProcessar = 5000;

		if (!int.TryParse(ConfigurationManager.AppSettings["TempoEspera"], out var tempoEspera))
			tempoEspera = 30;

		var pathProcessados = Path.Combine(caminhoArquivos, "processados");

		if (!Directory.Exists(pathProcessados))
			Directory.CreateDirectory(pathProcessados);

		var layoutsFatura = Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(IFatura).IsAssignableFrom(type) && !type.IsInterface).ToList();

		var layoutsProcessar = new[] { new { extensaoArquivo = "", tipo = typeof(NullabilityInfo), nomeLayout = "" } }.ToList();
		layoutsProcessar.Clear();

		Parallel.ForEach(layoutsFatura, x =>
		{
			try
			{
				var methodInfo = x.GetMethod("GetExtensaoArquivo");

				var classInstance = Activator.CreateInstance(x);

				var extensao = Convert.ToString(methodInfo.Invoke(classInstance, null));

				if (!string.IsNullOrWhiteSpace(extensao))
					layoutsProcessar.Add(new { extensaoArquivo = extensao, tipo = x, nomeLayout = x.Name });
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Erro ao recuperar extensão do layout {x.Name} - {ex.Message}");
			}
		});

		var files = Directory.GetFiles(caminhoArquivos).ToList();

		files.ForEach(x =>
		{
			try
			{
				var fileExtension = Path.GetExtension(x);

				var layout = layoutsProcessar.FirstOrDefault(x => fileExtension.ToUpper().EndsWith(x.extensaoArquivo.ToUpper()));

				if (layout != null)
				{
					var type = layout.tipo;

					var methodInfo = type.GetMethod("Processar");

					var classInstance = Activator.CreateInstance(type);

					Console.WriteLine($"Processando arquvo {x}");

					var result = Convert.ToBoolean(methodInfo.Invoke(classInstance, new object[] { x, nrRegistrosProcessar }));

					if (result)
					{
						var fi = new FileInfo(x);
						File.Move(x, Path.Combine(pathProcessados, fi.Name));
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Erro ao processar arquivo {x} - {ex.Message}");
			}
		});
	}
	catch (Exception ex)
	{
		Console.WriteLine(ex.Message);
	}
}