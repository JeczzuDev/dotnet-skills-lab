# DashboardLab - Interactive Data Dashboard

## Overview

Interactive web dashboard that integrates and visualizes data from multiple .NET projects (WebScraperLab and OCRLab) using a modern ASP.NET Core Web API backend and vanilla JavaScript frontend.

## Learning Objectives

- **Frontend Integration**: Connect HTML/CSS/JavaScript with .NET APIs
- **DOM Manipulation**: Dynamic content rendering and event handling
- **API Communication**: Fetch API with async/await patterns
- **Data Persistence**: LocalStorage for client-side data management
- **File Upload**: Multi-file handling with FormData and progress tracking
- **Responsive Design**: Mobile-first CSS with Grid and Flexbox
- **User Experience**: Loading states, modals, search, filtering, and sorting
- **CORS Configuration**: Cross-origin resource sharing in ASP.NET Core

## Architecture

```
DashboardLab/
├── src/
│   ├── DashboardApi/              # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── ScraperController.cs    # Reads books_output.csv
│   │   │   └── OcrController.cs        # Processes image uploads
│   │   ├── Services/
│   │   │   ├── IOcrService.cs          # OCR service interface
│   │   │   └── OcrService.cs           # Tesseract OCR implementation
│   │   ├── Models/
│   │   │   ├── Book.cs                 # Web scraper data model
│   │   │   └── OcrResult.cs            # OCR result data model
│   │   └── Program.cs                  # API configuration
│   └── frontend/                  # HTML, CSS, JavaScript
│       ├── index.html             # Dashboard UI
│       ├── styles.css             # Dark theme styling
│       └── app.js                 # Interactive logic
```

## Features Implemented

### Backend (ASP.NET Core Web API)
- **RESTful API Endpoints**: GET for data retrieval, POST for file uploads
- **OCR Integration**: Tesseract 5.2.0 for text extraction from images
- **File Upload Handling**: Multi-file processing with validation
- **CSV File Reading**: Loads web scraping results from WebScraperLab
- **CORS Configuration**: Allows frontend communication
- **Swagger Documentation**: Interactive API testing interface
- **Error Handling**: Graceful failures with empty data returns
- **Professional Logging**: Structured logs with context

### Frontend (HTML, CSS, JavaScript)
- **Dark Theme**: GitHub-inspired color scheme, no gradients or shadows
- **Responsive Layout**: CSS Grid dashboard adapts to all screen sizes
- **Image Upload**: Multi-file selection with drag-and-drop support
- **Progress Tracking**: Real-time upload progress bar with file count
- **LocalStorage Persistence**: OCR results persist across page reloads
- **Text Truncation**: Long text limited to 100 chars with "..." indicator
- **Modal Display**: Click truncated text to view full content
- **Search & Filter**: Real-time filtering by text, rating, and confidence
- **Table Sorting**: Click column headers to sort data
- **Empty States**: User guidance when no data available
- **Clear Functionality**: Button to remove all OCR results with confirmation
- **Event Delegation**: Secure event handling without inline onclick
- **Loading States**: Animated indicators during API requests

## Data Integration

### WebScraperLab Integration
- **Source**: Reads `books_output.csv` from WebScraperLab project
- **Display**: Shows real scraped book data (title, price, rating, availability)
- **Behavior**: Returns empty list if CSV not found (no mock data)
- **Updates**: Automatically reflects new data when WebScraperLab is re-executed

### OCRLab Integration
- **Dual Source Architecture**:
  1. **OCRLab Historical Data**: Reads `ocr_results.json` from OCRLab project
  2. **Dashboard Uploads**: Processes images uploaded directly in the dashboard
- **Data Persistence**:
  - OCRLab results: Loaded from JSON file on each page load
  - Dashboard results: Saved to localStorage and persist across sessions
- **Combined Display**: Shows both data sources unified in a single table
- **Source Tracking**: Results marked with `source` property (`'ocrlab'` or `'dashboard'`)
- **Storage Strategy**: Only dashboard uploads are saved to localStorage, OCRLab data is read-only

## How to Run

### Prerequisites
- .NET 8 SDK installed
- Tesseract OCR installed (configured in `appsettings.Development.json`)
- **WebScraperLab** executed at least once to generate `books_output.csv`
- **OCRLab** executed at least once to generate `ocr_results.json` (optional)

### 1. Start the Backend API
```bash
cd Learning/DashboardLab/src/DashboardApi
dotnet run
```
API will be available at `http://localhost:5000`

### 2. Start the Frontend Server
```bash
cd Learning/DashboardLab/src/frontend
python -m http.server 8080
```
Dashboard will be available at `http://localhost:8080`

### 3. View Results
- Dashboard UI: `http://localhost:8080`
- API Documentation: `http://localhost:5000/swagger`

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/scraper` | Returns books from WebScraperLab CSV |
| GET | `/api/ocr` | Returns OCR results from OCRLab JSON file |
| POST | `/api/ocr/upload` | Processes uploaded images with OCR |

## Technologies Used

- **Backend**: .NET 8, ASP.NET Core Web API, Tesseract 5.2.0, Swashbuckle 6.5.0
- **Frontend**: HTML5, CSS3, Vanilla JavaScript (ES6+)
- **Styling**: CSS Grid, Flexbox, CSS Variables (Dark Theme)
- **Storage**: LocalStorage API for client-side persistence
- **Data**: Fetch API, FormData, JSON, async/await

## Key Concepts Demonstrated

### Frontend Development
- **DOM Manipulation**: Dynamic table rendering with data-attributes
- **Event Handling**: Debounced search, delegated events, modal controls
- **Async Programming**: Promise handling, parallel data loading, error recovery
- **State Management**: Dual-source data combining (localStorage + API)
- **File Handling**: Multi-file uploads with FormData and progress tracking
- **User Experience**: Loading states, error messages, empty states, confirmations
- **Data Merging**: Combining results from multiple sources with source tracking

### Backend Development
- **Web API Design**: RESTful endpoints with proper HTTP methods
- **File Processing**: Image validation, temp file handling, cleanup
- **Configuration**: appsettings.json for environment-specific settings
- **Dependency Injection**: Service registration and interface patterns
- **CORS Policy**: Cross-origin request configuration

### Code Quality
- **Security**: Event delegation prevents XSS attacks
- **Performance**: Debouncing, efficient re-renders, for...of loops
- **Best Practices**: Number.parseInt(), replaceAll(), await RunAsync()
- **Error Handling**: Try-catch blocks with user-friendly messages

## Differences from Other Labs

| Aspect | OCRLab | WebScraperLab | DashboardLab |
|--------|--------|---------------|--------------|
| **OcrService** | Original implementation | N/A | Independent copy |
| **Data Storage** | JSON files | CSV file | LocalStorage + JSON/CSV reading |
| **Output** | Console application | Console application | Web dashboard |
| **OCR Data** | Generates `ocr_results.json` | N/A | Reads JSON + processes uploads |
| **Scraper Data** | N/A | Generates `books_output.csv` | Reads CSV |
| **Integration** | Standalone | Standalone | Consumes data from both labs |

---

**Status**: Completed
**Project Type**: Interactive Web Application  
**Architecture**: Client-Server with File Integration