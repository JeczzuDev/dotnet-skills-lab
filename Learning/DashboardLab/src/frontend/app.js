// API Configuration
const API_BASE_URL = 'http://localhost:5000/api';

// State Management
let booksData = [];
let ocrData = [];
let currentSortColumn = null;
let currentSortDirection = 'asc';
let selectedFiles = [];

// LocalStorage Keys
const STORAGE_KEY = 'dashboardlab_ocr_results';

// Initialize Dashboard
document.addEventListener('DOMContentLoaded', () => {
  initializeEventListeners();
  loadAllData();
});

// Event Listeners Setup
function initializeEventListeners() {
  // Search inputs
  document.getElementById('bookSearch').addEventListener('input', debounce(filterBooks, 300));
  document.getElementById('ocrSearch').addEventListener('input', debounce(filterOcrResults, 300));

  // Filter selects
  document.getElementById('ratingFilter').addEventListener('change', filterBooks);
  document.getElementById('confidenceFilter').addEventListener('change', filterOcrResults);

  // Upload controls
  document.getElementById('imageUpload').addEventListener('change', handleFileSelect);
  document.getElementById('uploadButton').addEventListener('click', handleUpload);
  document.getElementById('clearOcrButton').addEventListener('click', clearOcrResults);

  // Modal controls
  document.getElementById('closeModal').addEventListener('click', closeModal);
  document.getElementById('textModal').addEventListener('click', (e) => {
    if (e.target.id === 'textModal') closeModal();
  });

  // Table sorting
  for (const header of document.querySelectorAll('.sortable')) {
    header.addEventListener('click', handleSort);
  }
}

// Data Loading Functions
async function loadAllData() {
  await Promise.all([
    loadBooks(),
    loadOcrResults()
  ]);
}

async function loadBooks() {
  const loadingEl = document.getElementById('booksLoading');
  const errorEl = document.getElementById('booksError');
  const tableEl = document.getElementById('booksTable');

  try {
    loadingEl.style.display = 'block';
    errorEl.style.display = 'none';

    const response = await fetch(`${API_BASE_URL}/scraper`);

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    booksData = await response.json();

    loadingEl.style.display = 'none';
    tableEl.style.display = 'table';

    renderBooks(booksData);
  } catch (error) {
    console.error('Failed to load books:', error);
    loadingEl.style.display = 'none';
    errorEl.textContent = `Failed to load books: ${error.message}. Please ensure the API is running on port 5000.`;
    errorEl.style.display = 'block';
  }
}

async function loadOcrResults() {
  const loadingEl = document.getElementById('ocrLoading');
  const errorEl = document.getElementById('ocrError');
  const tableEl = document.getElementById('ocrTable');

  try {
    loadingEl.style.display = 'block';
    errorEl.style.display = 'none';

    const storedData = localStorage.getItem(STORAGE_KEY);
    const dashboardResults = storedData ? JSON.parse(storedData) : [];

    let ocrLabResults = [];
    try {
      const response = await fetch(`${API_BASE_URL}/ocr`);
      if (response.ok) {
        ocrLabResults = await response.json();
        ocrLabResults = ocrLabResults.map(r => ({ ...r, source: 'ocrlab' }));
      }
    } catch (apiError) {
      console.warn('Could not load OCRLab results:', apiError.message);
    }

    ocrData = [...dashboardResults, ...ocrLabResults];

    loadingEl.style.display = 'none';
    tableEl.style.display = 'table';

    renderOcrResults(ocrData);
  } catch (error) {
    console.error('Failed to load OCR results:', error);
    loadingEl.style.display = 'none';
    errorEl.textContent = `Failed to load OCR results: ${error.message}`;
    errorEl.style.display = 'block';
  }
}

// Rendering Functions
function renderBooks(books) {
  const tbody = document.getElementById('booksTableBody');
  const countEl = document.getElementById('booksCount');

  if (books.length === 0) {
    tbody.innerHTML = '<tr><td colspan="4" style="text-align: center; padding: 2rem; color: #6b7280;">No books found</td></tr>';
    countEl.textContent = '0 books';
    return;
  }

  tbody.innerHTML = books.map(book => `
        <tr>
            <td><strong>${escapeHtml(book.title)}</strong></td>
            <td>$${book.price.toFixed(2)}</td>
            <td>
                <span class="rating-badge rating-${book.rating}">
                    ${'★'.repeat(book.rating)}${'☆'.repeat(5 - book.rating)}
                </span>
            </td>
            <td>${escapeHtml(book.availability)}</td>
        </tr>
    `).join('');

  countEl.textContent = `${books.length} book${books.length === 1 ? '' : 's'}`;
}

function renderOcrResults(results) {
  const tbody = document.getElementById('ocrTableBody');
  const countEl = document.getElementById('ocrCount');
  const emptyState = document.getElementById('emptyState');
  const ocrTable = document.getElementById('ocrTable');
  const ocrLoading = document.getElementById('ocrLoading');
  const clearButton = document.getElementById('clearOcrButton');

  ocrLoading.style.display = 'none';

  if (results.length === 0) {
    ocrTable.style.display = 'none';
    emptyState.style.display = 'block';
    countEl.textContent = '0 results';
    clearButton.style.display = 'none';
    return;
  }

  emptyState.style.display = 'none';
  ocrTable.style.display = 'table';
  clearButton.style.display = 'block';

  tbody.innerHTML = results.map((result, index) => {
    const confidencePercent = (result.confidence * 100).toFixed(1);
    const confidenceClass = getConfidenceClass(result.confidence);

    // Build extracted text cell content
    let extractedTextCell;
    if (result.extractedText) {
      const text = result.extractedText;
      const maxLength = 100;
      const isTruncated = text.length > maxLength;
      const displayText = isTruncated ? text.substring(0, maxLength) + '...' : text;

      if (isTruncated) {
        extractedTextCell = `<span class="text-truncate" data-index="${index}">${escapeHtml(displayText)}</span>`;
      } else {
        extractedTextCell = `<span>${escapeHtml(displayText)}</span>`;
      }
    } else {
      extractedTextCell = '<em>No text extracted</em>';
    }

    return `
            <tr>
                <td><code>${escapeHtml(result.fileName)}</code></td>
                <td class="text-cell">${extractedTextCell}</td>
                <td>
                    <span class="confidence-badge ${confidenceClass}">
                        ${confidencePercent}%
                    </span>
                </td>
                <td>
                    <span class="status-badge ${result.success ? 'status-success' : 'status-failed'}">
                        ${result.success ? 'Success' : 'Failed'}
                    </span>
                </td>
            </tr>
        `;
  }).join('');

  // Add click event listeners to truncated text
  for (const span of document.querySelectorAll('.text-truncate')) {
    span.addEventListener('click', () => {
      const index = Number.parseInt(span.dataset.index);
      const result = results[index];
      openModal(result.fileName, result.extractedText);
    });
  }

  countEl.textContent = `${results.length} result${results.length === 1 ? '' : 's'}`;
}

// Filtering Functions
function filterBooks() {
  const searchTerm = document.getElementById('bookSearch').value.toLowerCase();
  const ratingFilter = document.getElementById('ratingFilter').value;

  let filtered = booksData;

  // Search filter
  if (searchTerm) {
    filtered = filtered.filter(book =>
      book.title.toLowerCase().includes(searchTerm) ||
      book.availability.toLowerCase().includes(searchTerm)
    );
  }

  // Rating filter
  if (ratingFilter) {
    filtered = filtered.filter(book => book.rating === Number.parseInt(ratingFilter));
  }

  renderBooks(filtered);
}

function filterOcrResults() {
  const searchTerm = document.getElementById('ocrSearch').value.toLowerCase();
  const confidenceFilter = document.getElementById('confidenceFilter').value;

  let filtered = ocrData;

  // Search filter
  if (searchTerm) {
    filtered = filtered.filter(result =>
      result.fileName.toLowerCase().includes(searchTerm) ||
      result.extractedText.toLowerCase().includes(searchTerm)
    );
  }

  // Confidence filter
  if (confidenceFilter) {
    filtered = filtered.filter(result => {
      if (confidenceFilter === 'high') return result.confidence > 0.9;
      if (confidenceFilter === 'medium') return result.confidence >= 0.7 && result.confidence <= 0.9;
      if (confidenceFilter === 'low') return result.confidence < 0.7;
      return true;
    });
  }

  renderOcrResults(filtered);
}

// Sorting Functions
function handleSort(event) {
  const column = event.target.dataset.column;
  const table = event.target.closest('table').id;

  // Toggle sort direction
  if (currentSortColumn === column) {
    currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
  } else {
    currentSortColumn = column;
    currentSortDirection = 'asc';
  }

  // Update UI indicators
  for (const th of document.querySelectorAll('.sortable')) {
    th.textContent = th.textContent.replaceAll(/[⬆⬇⬍]/g, '') + ' ⬍';
  }
  event.target.textContent = event.target.textContent.replaceAll(/[⬆⬇⬍]/g, '') +
    (currentSortDirection === 'asc' ? ' ⬆' : ' ⬇');

  // Sort appropriate dataset
  if (table === 'booksTable') {
    sortBooks(column);
  } else if (table === 'ocrTable') {
    sortOcrResults(column);
  }
}

function sortBooks(column) {
  const sorted = [...booksData].sort((a, b) => {
    let aVal = a[column];
    let bVal = b[column];

    if (typeof aVal === 'string') {
      aVal = aVal.toLowerCase();
      bVal = bVal.toLowerCase();
    }

    if (aVal < bVal) return currentSortDirection === 'asc' ? -1 : 1;
    if (aVal > bVal) return currentSortDirection === 'asc' ? 1 : -1;
    return 0;
  });

  booksData = sorted;
  filterBooks(); // Re-apply filters after sorting
}

function sortOcrResults(column) {
  const sorted = [...ocrData].sort((a, b) => {
    let aVal = a[column];
    let bVal = b[column];

    if (typeof aVal === 'string') {
      aVal = aVal.toLowerCase();
      bVal = bVal.toLowerCase();
    }

    if (aVal < bVal) return currentSortDirection === 'asc' ? -1 : 1;
    if (aVal > bVal) return currentSortDirection === 'asc' ? 1 : -1;
    return 0;
  });

  ocrData = sorted;
  filterOcrResults(); // Re-apply filters after sorting
}

// File Upload Functions
function handleFileSelect(event) {
  selectedFiles = Array.from(event.target.files);
  const fileCount = document.getElementById('fileCount');
  const uploadButton = document.getElementById('uploadButton');

  console.log(`[INFO] Files selected: ${selectedFiles.length}`);

  if (selectedFiles.length > 0) {
    fileCount.textContent = `${selectedFiles.length} file${selectedFiles.length === 1 ? '' : 's'} selected`;
    uploadButton.disabled = false;
    console.log(`[INFO] Upload button enabled`);
  } else {
    fileCount.textContent = '';
    uploadButton.disabled = true;
  }
}

async function handleUpload() {
  if (selectedFiles.length === 0) {
    console.log('[WARNING] No files selected');
    return;
  }

  console.log(`[INFO] Starting upload of ${selectedFiles.length} files`);

  const uploadButton = document.getElementById('uploadButton');
  const progressSection = document.getElementById('uploadProgress');
  const progressFill = document.getElementById('progressBarFill');
  const uploadStatus = document.getElementById('uploadStatus');
  const ocrTable = document.getElementById('ocrTable');

  uploadButton.disabled = true;
  progressSection.style.display = 'block';
  ocrTable.style.display = 'table';

  const totalFiles = selectedFiles.length;
  let processedFiles = 0;
  const newResults = [];

  uploadStatus.textContent = `Processing 0/${totalFiles} images...`;

  for (const file of selectedFiles) {
    try {
      console.log(`[INFO] Uploading ${file.name}...`);

      const formData = new FormData();
      formData.append('file', file);

      const response = await fetch(`${API_BASE_URL}/ocr/upload`, {
        method: 'POST',
        body: formData
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const result = await response.json();
      newResults.push(result);

      processedFiles++;
      const progress = (processedFiles / totalFiles) * 100;
      progressFill.style.width = `${progress}%`;
      uploadStatus.textContent = `Processing ${processedFiles}/${totalFiles} images...`;
    } catch (error) {
      console.error(`Failed to process ${file.name}:`, error);
      processedFiles++;

      newResults.push({
        fileName: file.name,
        extractedText: `Error: ${error.message}`,
        confidence: 0,
        success: false
      });
    }
  }

  const markedResults = newResults.map(r => ({ ...r, source: 'dashboard' }));

  ocrData = [...markedResults, ...ocrData];

  saveOcrResults();

  renderOcrResults(ocrData);

  uploadStatus.textContent = `Completed! Processed ${totalFiles} image${totalFiles === 1 ? '' : 's'}`;

  setTimeout(() => {
    progressSection.style.display = 'none';
    progressFill.style.width = '0%';
    uploadButton.disabled = false;

    // Reset file input
    document.getElementById('imageUpload').value = '';
    document.getElementById('fileCount').textContent = '';
    selectedFiles = [];
    uploadButton.disabled = true;
  }, 2000);
}

// Modal Functions
function openModal(fileName, text) {
  const modal = document.getElementById('textModal');
  const modalFileName = document.getElementById('modalFileName');
  const modalText = document.getElementById('modalText');

  modalFileName.textContent = fileName;
  modalText.textContent = text;
  modal.style.display = 'flex';
}

function closeModal() {
  const modal = document.getElementById('textModal');
  modal.style.display = 'none';
}

// Utility Functions
function saveOcrResults() {
  try {
    const dashboardResults = ocrData.filter(r => r.source === 'dashboard');
    localStorage.setItem(STORAGE_KEY, JSON.stringify(dashboardResults));
  } catch (error) {
    console.error('Failed to save to localStorage:', error);
  }
}

function clearOcrResults() {
  if (confirm('Are you sure you want to clear all OCR results? This action cannot be undone.')) {
    ocrData = [];
    localStorage.removeItem(STORAGE_KEY);
    renderOcrResults(ocrData);
  }
}

function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

function getConfidenceClass(confidence) {
  if (confidence > 0.9) return 'confidence-high';
  if (confidence >= 0.7) return 'confidence-medium';
  return 'confidence-low';
}

