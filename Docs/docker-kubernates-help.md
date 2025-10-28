# 🧱 DOCKER Y KUBERNETES – GUÍA DE CONFIGURACIÓN Y USO

## 📦 1. DOCKER Y DOCKER COMPOSE – Configuración paso a paso

### 🚀 Instalación inicial
1. Descargar **Docker Desktop** desde [https://www.docker.com/get-started](https://www.docker.com/get-started).  
2. Instalar y asegurarse de que Docker esté corriendo antes de usar Visual Studio.  
3. En Visual Studio, para cada proyecto:
   - Click derecho → **Add > Docker Support**  
   - Elegir **Linux** como sistema base.
4. Para orquestar varios servicios:
   - Click derecho en cada proyecto → **Add > Container Orchestrator Support > Docker Compose**  
   - Esto genera un proyecto `docker-compose.dcproj` y los archivos `docker-compose.yml` y `docker-compose.override.yml`.

---

### ⚙️ Comandos básicos (día a día)

| Comando | Explicación |
|----------|-------------|
| `docker build -t nombre_imagen .` | Compila una imagen Docker a partir del `Dockerfile` en el directorio actual. |
| `docker images` | Lista todas las imágenes locales disponibles. |
| `docker ps` | Muestra los contenedores en ejecución. |
| `docker ps -a` | Muestra todos los contenedores (en ejecución o detenidos). |
| `docker stop <id>` | Detiene un contenedor por ID o nombre. |
| `docker rm <id>` | Elimina un contenedor detenido. |
| `docker rmi <imagen>` | Elimina una imagen del repositorio local. |
| `docker logs <id>` | Muestra los logs del contenedor. |
| `docker exec -it <id> /bin/bash` | Entra a la terminal del contenedor. |
| `docker image prune -f` | Limpia imágenes no utilizadas (libera espacio). |
| `docker compose up --build` | Construye (si hace falta) y levanta todos los servicios definidos en `docker-compose.yml`. |
| `docker compose down` | Detiene y elimina todos los contenedores, redes y volúmenes definidos. |
| `docker compose ps` | Lista los servicios activos del Compose. |
| `docker compose logs` | Muestra logs combinados de todos los servicios. |
| `docker compose restart` | Reinicia los servicios activos del Compose. |

---

### 🔁 Flujo usual de trabajo (día a día)

1. **Actualizar código fuente**  
   (Visual Studio o Git pull)

2. **Reconstruir y levantar todo**
   ```bash
   docker compose up --build
   ```

3. **Ver logs**
   ```bash
   docker compose logs
   ```

4. **Detener entorno**
   ```bash
   docker compose down
   ```

---

## ☸️ 2. KUBERNETES – Configuración paso a paso

### ⚙️ Instalación inicial
1. En Docker Desktop → activar **Kubernetes**  
   (Settings → Kubernetes → Enable).  
2. Esperar a que el estado cambie a **Kubernetes is running**.  
3. Verificar instalación:
   ```bash
   kubectl version
   ```

---

### 🧩 Configuración inicial

#### Crear namespace (solo una vez)
```bash
kubectl create namespace lab-slingr
# Crea un espacio lógico dentro del cluster para tus microservicios
```

#### Estructura recomendada
```
k8s/
 ├─ namespace.yaml
 ├─ authservice-deployment.yaml
 ├─ authservice-service.yaml
 ├─ employeeservice-deployment.yaml
 ├─ employeeservice-service.yaml
 └─ (futuros redis / sqlserver ...)
```

#### Aplicar todos los YAML
```bash
kubectl apply -f k8s/
# Crea o actualiza todos los objetos definidos (deployments, services, etc.)
```

---

### ⚙️ Comandos básicos de Kubernetes

| Comando | Explicación |
|----------|-------------|
| `kubectl get nodes` | Lista los nodos del cluster. |
| `kubectl get namespaces` | Muestra todos los namespaces existentes. |
| `kubectl get all -n lab-slingr` | Lista todos los pods, deployments y servicios del namespace. |
| `kubectl get pods -n lab-slingr` | Muestra los pods activos en el namespace. |
| `kubectl get svc -n lab-slingr` | Muestra los servicios expuestos (puertos NodePort, IPs internas). |
| `kubectl logs deployment/authservice -n lab-slingr` | Muestra logs de un deployment específico. |
| `kubectl describe pod <nombre> -n lab-slingr` | Detalla el estado y eventos de un pod (útil para debug). |
| `kubectl delete pod <nombre> -n lab-slingr` | Borra un pod (se recrea automáticamente según el deployment). |
| `kubectl delete deployment <nombre> -n lab-slingr` | Elimina un deployment. |
| `kubectl apply -f k8s/<archivo>.yaml` | Crea o actualiza un recurso específico. |
| `kubectl rollout restart deployment/<nombre> -n lab-slingr` | Reinicia un deployment (nuevo contenedor con misma imagen). |

---

### 🔁 Flujo usual de trabajo (día a día)

#### 1️⃣ Re-construir imágenes locales
```bash
docker build -t authserviceapi:dev -f AuthService/src/AuthService.Api/Dockerfile .
docker build -t employeeserviceapi:dev -f EmployeeService/src/EmployeeService.Api/Dockerfile .
# Compila las nuevas imágenes de cada microservicio
```

#### 2️⃣ Aplicar cambios al cluster
```bash
kubectl apply -f k8s/
# Actualiza los deployments con las nuevas imágenes
```

#### 3️⃣ Verificar estado
```bash
kubectl get pods -n lab-slingr
# Esperar a que todos estén en estado Running
```

#### 4️⃣ Consultar servicios activos
```bash
kubectl get svc -n lab-slingr
# Ver puertos expuestos (ej: 30002, 30003)
```

#### 5️⃣ Probar APIs
```
http://localhost:30002/swagger   → AuthService
http://localhost:30003/swagger   → EmployeeService
```

---

### 🧹 Limpieza del entorno (opcional)
```bash
kubectl delete all --all -n lab-slingr
# Borra todos los recursos del namespace

kubectl delete namespace lab-slingr
# Elimina completamente el namespace (para reiniciar todo desde cero)
```

---

## 🧠 3. Flujo resumido de despliegue real (conceptual)

| Etapa | Herramienta | Comando / Acción | Resultado |
|--------|--------------|------------------|------------|
| **Desarrollo** | Visual Studio / Docker Compose | `docker compose up --build` | Corre entorno local de dev |
| **Infra local (testing)** | Kubernetes local | `kubectl apply -f k8s/` | Simula producción |
| **Pipeline CI/CD** | DevOps / GitHub Actions | `docker build + push + kubectl apply` | Despliega a cluster cloud |
| **Producción** | Kubernetes cloud (AKS, EKS) | Deploy automático | Entorno final en la nube |

---

## 🚀 4. Recordatorio rápido de mantenimiento

| Acción | Comando | Qué hace |
|---------|----------|----------|
| Ver estado general del cluster | `kubectl get all -n lab-slingr` | Muestra todo lo que corre en el namespace. |
| Ver logs de un servicio | `kubectl logs deployment/authservice -n lab-slingr` | Permite revisar la salida del contenedor. |
| Reiniciar servicios sin borrar | `kubectl rollout restart deployment/<nombre> -n lab-slingr` | Refresca los pods con la misma imagen. |
| Limpiar contenedores viejos de Docker | `docker image prune -f` | Libera espacio local. |
| Eliminar y recrear todo el entorno | `kubectl delete all --all -n lab-slingr` y luego `kubectl apply -f k8s/` | Reinicia desde cero. |

---

✅ **Con esto tenés tu entorno de desarrollo, pruebas y despliegue totalmente documentado.**  
Podés usar este archivo como *README técnico* de infraestructura o guía interna para el equipo.
