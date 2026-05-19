import { useState, useEffect } from "react";

const API = window.location.hostname === "localhost" 
  ? "http://localhost:8080/api/products"
  : "/api/products";

const categoryColors = {
  Dairy: "#E1F5EE",
  Bakery: "#FAEEDA",
  Seafood: "#E6F1FB",
  Produce: "#EAF3DE",
  Meat: "#FAECE7",
};

export default function App() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [form, setForm] = useState({ name: "", category: "", quantity: "", price: "" });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const fetchProducts = async () => {
    try {
      const res = await fetch(API);
      const data = await res.json();
      setProducts(data);
    } catch (e) {
      setError("Cannot reach API");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchProducts(); }, []);

  const handleAdd = async () => {
    if (!form.name || !form.category || !form.quantity || !form.price) return;
    setSubmitting(true);
    await fetch(API, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        name: form.name,
        category: form.category,
        quantity: parseInt(form.quantity),
        price: parseFloat(form.price),
      }),
    });
    setForm({ name: "", category: "", quantity: "", price: "" });
    await fetchProducts();
    setSubmitting(false);
  };

  const handleDelete = async (id) => {
    await fetch(`${API.replace("/api/products", "")}/api/products/${id}`, { method: "DELETE" });
    await fetchProducts();
  };

  const filtered = products.filter(p =>
    p.name.toLowerCase().includes(search.toLowerCase()) ||
    p.category.toLowerCase().includes(search.toLowerCase())
  );

  const totalItems = products.reduce((s, p) => s + p.quantity, 0);
  const outOfStock = products.filter(p => !p.inStock).length;
  const totalValue = products.reduce((s, p) => s + p.price * p.quantity, 0);

  return (
    <div style={{ fontFamily: "system-ui, sans-serif", maxWidth: 900, margin: "0 auto", padding: "2rem 1rem", color: "#1a1a1a" }}>
      
      <div style={{ marginBottom: "2rem" }}>
        <h1 style={{ fontSize: 24, fontWeight: 600, margin: 0 }}>Heinen's Inventory</h1>
        <p style={{ color: "#666", margin: "4px 0 0", fontSize: 14 }}>Store product tracker</p>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 12, marginBottom: "2rem" }}>
        {[
          { label: "Total units", value: totalItems.toLocaleString() },
          { label: "Out of stock", value: outOfStock },
          { label: "Inventory value", value: `$${totalValue.toFixed(2)}` },
        ].map(({ label, value }) => (
          <div key={label} style={{ background: "#f7f7f5", borderRadius: 10, padding: "1rem", textAlign: "center" }}>
            <div style={{ fontSize: 13, color: "#666", marginBottom: 4 }}>{label}</div>
            <div style={{ fontSize: 22, fontWeight: 600 }}>{value}</div>
          </div>
        ))}
      </div>

      <div style={{ background: "#fff", border: "0.5px solid #e0e0e0", borderRadius: 12, padding: "1.25rem", marginBottom: "1.5rem" }}>
        <div style={{ fontWeight: 500, marginBottom: 12, fontSize: 14 }}>Add product</div>
        <div style={{ display: "grid", gridTemplateColumns: "2fr 1fr 1fr 1fr auto", gap: 8 }}>
          {[
            { key: "name", placeholder: "Product name" },
            { key: "category", placeholder: "Category" },
            { key: "quantity", placeholder: "Qty", type: "number" },
            { key: "price", placeholder: "Price", type: "number" },
          ].map(({ key, placeholder, type }) => (
            <input
              key={key}
              type={type || "text"}
              placeholder={placeholder}
              value={form[key]}
              onChange={e => setForm(f => ({ ...f, [key]: e.target.value }))}
              style={{ padding: "8px 10px", borderRadius: 8, border: "0.5px solid #ddd", fontSize: 13, outline: "none" }}
            />
          ))}
          <button
            onClick={handleAdd}
            disabled={submitting}
            style={{ padding: "8px 16px", borderRadius: 8, border: "none", background: "#1D9E75", color: "#fff", fontWeight: 500, fontSize: 13, cursor: "pointer" }}
          >
            {submitting ? "..." : "Add"}
          </button>
        </div>
      </div>

      <input
        placeholder="Search products or categories..."
        value={search}
        onChange={e => setSearch(e.target.value)}
        style={{ width: "100%", padding: "10px 14px", borderRadius: 10, border: "0.5px solid #ddd", fontSize: 14, marginBottom: "1rem", boxSizing: "border-box", outline: "none" }}
      />

      {loading && <div style={{ textAlign: "center", color: "#666", padding: "2rem" }}>Loading...</div>}
      {error && <div style={{ textAlign: "center", color: "#e24b4a", padding: "2rem" }}>{error}</div>}

      <div style={{ display: "grid", gap: 8 }}>
        {filtered.map(p => (
          <div key={p.id} style={{ background: "#fff", border: "0.5px solid #e0e0e0", borderRadius: 12, padding: "1rem 1.25rem", display: "flex", alignItems: "center", gap: 12 }}>
            <div style={{ background: categoryColors[p.category] || "#f0f0f0", borderRadius: 8, padding: "6px 12px", fontSize: 12, fontWeight: 500, color: "#333", minWidth: 70, textAlign: "center" }}>
              {p.category}
            </div>
            <div style={{ flex: 1 }}>
              <div style={{ fontWeight: 500, fontSize: 14 }}>{p.name}</div>
              <div style={{ fontSize: 12, color: "#888", marginTop: 2 }}>${p.price.toFixed(2)} each</div>
            </div>
            <div style={{ textAlign: "right", marginRight: 12 }}>
              <div style={{ fontWeight: 600, fontSize: 15 }}>{p.quantity}</div>
              <div style={{ fontSize: 11, color: p.inStock ? "#1D9E75" : "#e24b4a", fontWeight: 500 }}>
                {p.inStock ? "in stock" : "out of stock"}
              </div>
            </div>
            <button
              onClick={() => handleDelete(p.id)}
              style={{ background: "none", border: "0.5px solid #e0e0e0", borderRadius: 8, padding: "6px 10px", cursor: "pointer", color: "#999", fontSize: 12 }}
            >
              remove
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
