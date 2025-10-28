# 🧱 DOCKER Y KUBERNETES -- GUÍA DE CONFIGURACIÓN Y USO

## 📦 1. DOCKER Y DOCKER COMPOSE -- Configuración paso a paso

### 🚀 Instalación inicial

1.  Descargar **Docker Desktop** desde
    https://www.docker.com/get-started\
2.  Instalar y asegurarse de que Docker esté corriendo.\
3.  En Visual Studio, por cada proyecto:
    -   Click derecho → **Add \> Docker Support**
    -   Elegir **Linux**
4.  Para orquestar múltiples servicios:
    -   Click derecho → **Add \> Container Orchestrator Support \>
        Docker Compose**
    -   Esto genera `docker-compose.dcproj`, `docker-compose.yml` y
        `docker-compose.override.yml`.

------------------------------------------------------------------------

### ⚙️ Comandos básicos (día a día)

  -------------------------------------------------------------------------------
  Comando                             Explicación
  ----------------------------------- -------------------------------------------
  `docker build -t nombre_imagen .`   Construye la imagen a partir del
                                      Dockerfile.

  `docker images`                     Lista imágenes locales.

  `docker ps`                         Contenedores activos.

  `docker ps -a`                      Todos los contenedores existentes.

  `docker stop <id>`                  Detiene un contenedor.

  `docker rm <id>`                    Elimina contenedor detenido.

  `docker rmi <imagen>`               Borra una imagen.

  `docker logs <id>`                  Logs.

  `docker exec -it <id> /bin/bash`    Entra al contenedor.

  `docker image prune -f`             Limpia imágenes sin uso.

  `docker compose up --build`         Levanta todo reconstruyendo.

  `docker compose down`               Detiene y borra entorno.

  `docker compose ps`                 Lista servicios.

  `docker compose logs`               Logs combinados.

  `docker compose restart`            Reinicia servicios.
  -------------------------------------------------------------------------------

------------------------------------------------------------------------

### 🔁 Flujo típico

``` bash
docker compose up --build
docker compose logs
docker compose down
```

------------------------------------------------------------------------

## ☸️ 2. KUBERNETES -- Configuración paso a paso

### 🚀 Instalación inicial

1.  Docker Desktop → Settings → Kubernetes → **Enable**
2.  Esperar: **Kubernetes is running**
3.  Verificar:

``` bash
kubectl version
```

------------------------------------------------------------------------

### 🧩 Configuración inicial

#### Crear namespace

``` bash
kubectl create namespace lab-generic
```

#### Estructura recomendada

    k8s/
     ├─ namespace.yaml
     ├─ authservice-deployment.yaml
     ├─ authservice-service.yaml
     ├─ employeeservice-deployment.yaml
     ├─ employeeservice-service.yaml

#### Aplicar todo

``` bash
kubectl apply -f k8s/
```

------------------------------------------------------------------------

### ⚙️ Comandos básicos Kubernetes

  --------------------------------------------------------------------------------------------------------
  Comando                                                      Explicación
  ------------------------------------------------------------ -------------------------------------------
  `kubectl get nodes`                                          Nodos del cluster

  `kubectl get namespaces`                                     Namespaces

  `kubectl get all -n lab-generic`                             Todo el namespace

  `kubectl get pods -n lab-generic`                            Pods

  `kubectl get svc -n lab-generic`                             Servicios

  `kubectl logs deployment/authservice -n lab-generic`         Logs

  `kubectl describe pod <pod> -n lab-generic`                  Detalle del pod

  `kubectl delete pod <pod> -n lab-generic`                    Elimina pod

  `kubectl delete deployment <nombre> -n lab-generic`          Elimina deployment

  `kubectl apply -f k8s/<archivo>`                             Aplica cambios

  `kubectl rollout restart deployment/<name> -n lab-generic`   Reinicia deployment
  --------------------------------------------------------------------------------------------------------

------------------------------------------------------------------------

### 🔁 Flujo diario

#### 1. Build imágenes

``` bash
docker build -t auth-service.api:dev `-f Services/AuthService/src/AuthService.API/Dockerfile`
docker build -t employee-service.api:dev `-f Services/EmployeeService/src/EmployeeService.API/Dockerfile`
```

#### 2. Aplicar

``` bash
kubectl apply -f k8s/
```

#### 3. Estado

``` bash
kubectl get pods -n lab-generic
```

#### 4. Servicios:

``` bash
kubectl get svc -n lab-generic
```

#### 5. Swagger:

    http://localhost:30002/swagger
    http://localhost:30003/swagger

------------------------------------------------------------------------

### 🧹 Limpieza

``` bash
kubectl delete all --all -n lab-generic
kubectl delete namespace lab-generic
```

------------------------------------------------------------------------

## 🧠 3. Flujo de despliegue

  Etapa           Herramienta             Acción
  --------------- ----------------------- -----------------------------
  Desarrollo      Docker Compose          `docker compose up --build`
  Testing local   Kubernetes              `kubectl apply -f k8s/`
  CI/CD           GitHub / Azure DevOps   Build + Push + Deploy
  Producción      AKS/EKS                 Rollout automático

------------------------------------------------------------------------

## 🚀 4. Mantenimiento rápido

  ------------------------------------------------------------------------------------------------------
  Acción                            Comando
  --------------------------------- --------------------------------------------------------------------
  Estado general                    `kubectl get all -n lab-generic`

  Logs                              `kubectl logs deployment/authservice -n lab-generic`

  Restart                           `kubectl rollout restart deployment/<name> -n lab-generic`

  Limpiar Docker                    `docker image prune -f`

  Resetear entorno                  `kubectl delete all --all -n lab-generic && kubectl apply -f k8s/`
  ------------------------------------------------------------------------------------------------------

------------------------------------------------------------------------

# 🧪 5. CONFIGURACIÓN DE UNIT TESTS & INTEGRATION TESTS

## 🧩 5.1 UNIT TESTS

### 📦 Paquetes necesarios

``` bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package FluentAssertions
dotnet add package Moq
```

### 🔗 Referencias

``` xml
<ItemGroup>
    <ProjectReference Include="..\..\src\EmployeeService.Application\EmployeeService.Application.csproj" />
    <ProjectReference Include="..\..\src\EmployeeService.API\EmployeeService.API.csproj" />
    <ProjectReference Include="..\..\src\EmployeeService.Domain\EmployeeService.Domain.csproj" />
</ItemGroup>
```

### ✔️ Detalles

-   Se mockea `IEmployeeService`
-   Solo se testean controllers
-   No levanta API real
-   FluentAssertions para aserciones

### 🧪 Ejemplo unit test

``` csharp
[Fact]
public async Task GetById_ShouldReturnOk_WhenEmployeeExists()
{
    var id = Guid.NewGuid();
    var employee = new Employee { Id = id, FirstName = "Juan" };

    _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(employee);

    var result = await _controller.GetById(id);

    var okResult = result.Result as OkObjectResult;
    okResult.Should().NotBeNull();
}
```

------------------------------------------------------------------------

## ☄️ 5.2 INTEGRATION TESTS

### 🔥 Paso obligatorio (TODOS los microservicios)

Agregar al **final** de Program.cs:

``` csharp
public partial class Program { }
```

Debe ir después de:

``` csharp
app.Run();
```

Si falta → error:

    Can't find testhost.deps.json

------------------------------------------------------------------------

### 📦 Paquetes necesarios

``` bash
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package FluentAssertions
```

------------------------------------------------------------------------

### 🔗 Referencia al API

``` xml
<ItemGroup>
  <ProjectReference Include="..\..\src\EmployeeService.API\EmployeeService.API.csproj" />
</ItemGroup>
```

------------------------------------------------------------------------

### 🌐 Constructor de Integration Test

``` csharp
public class EmployeeApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EmployeeApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
}
```

------------------------------------------------------------------------

### 🧪 Ejemplo

``` csharp
[Fact]
public async Task GetAll_ShouldReturnOk()
{
    var response = await _client.GetAsync("/api/employees");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

------------------------------------------------------------------------

## 🧱 5.3 Autenticación en tests

### ✔️ Opción A -- Desactivar auth

**En Program.cs:**

``` csharp
if (!Environment.GetEnvironmentVariable("DISABLE_AUTH")?.Equals("true") ?? false)
{
    app.UseAuthentication();
}
```

**En test:**

``` csharp
Environment.SetEnvironmentVariable("DISABLE_AUTH", "true");
```

### ✔️ Opción B -- Token dummy

``` csharp
_client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", "fake-token");
```

------------------------------------------------------------------------

## 🧠 5.4 Resumen final

  Requisito                              Estado
  -------------------------------------- -----------------
  `public partial class Program { }`     ✔️ Obligatorio
  `Microsoft.AspNetCore.Mvc.Testing`     ✔️ Obligatorio
  Referencia al proyecto API             ✔️ Obligatorio
  Autenticación desactivada o mockeada   👍 Recomendado
  PreserveCompilationContext             ❌ No necesario
  ContentRoot manual                     ❌ No necesario
